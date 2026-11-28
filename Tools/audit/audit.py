#!/usr/bin/env python3
"""Offline pre-import project audit for Dude Chaos Wars (§4.3 & Engineering Rules 4-5).

Checks:
  - ASMDEF_PARSE: valid JSON + assembly name on every *.asmdef under Assets/
  - ASMDEF_DUPLICATE: unique assembly names
  - ASMDEF_CYCLE: DFS cycle detection over the assembly reference graph
  - ASMDEF_DANGLING_REF: every referenced DudeChaos.* assembly exists
  - PACKAGE_NOT_IN_MANIFEST: Unity package assemblies backed by Packages/manifest.json
  - SCRIPT_UNCOVERED: every *.cs under Assets/ covered by an ancestor *.asmdef
  - DUPLICATE_TYPE: duplicate top-level type declarations across Assets/
  - USING_NOT_REFERENCED: using DudeChaos.X must be in that assembly's references
  - DUPLICATE_GUID: duplicate GUIDs across *.meta files under Assets/
  - ASSET_SCRIPT_GUID: every *.asset m_Script GUID resolves to a *.cs.meta (or URP/TMP package whitelist when Library/PackageCache is absent)
  - ASSET_FIELD_MISMATCH: every serialized field on a ScriptableObject *.asset exists on its C# class
  - ASSET_VECTOR: every {x:..., y:...[, z:...][, w:...]} and AnimationCurve keyframe value is finite numeric
  - PROJECT_VERSION: ProjectSettings/ProjectVersion.txt locked to 6000.3.28f1
"""

from __future__ import annotations

import hashlib
import json
import math
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent

UNITY_PACKAGE_ASM_TO_MANIFEST: dict[str, tuple[str, ...]] = {
    "Unity.InputSystem": ("com.unity.inputsystem",),
    "Unity.Cinemachine": ("com.unity.cinemachine",),
    "UnityEngine.UI": ("com.unity.ugui",),
    # In Unity 6 (6000.3 LTS), TextMeshPro ships inside com.unity.ugui 2.0.0.
    "Unity.TextMeshPro": ("com.unity.ugui", "com.unity.textmeshpro"),
    "Unity.RenderPipelines.Core.Runtime": ("com.unity.render-pipelines.core",),
    "Unity.RenderPipelines.Core.Editor": ("com.unity.render-pipelines.core",),
    "Unity.RenderPipelines.Universal.Runtime": ("com.unity.render-pipelines.universal",),
    "Unity.RenderPipelines.Universal.Editor": ("com.unity.render-pipelines.universal",),
    "UnityEngine.TestRunner": ("com.unity.test-framework",),
    "UnityEditor.TestRunner": ("com.unity.test-framework",),
}

BUILTIN_UNITY_ASSEMBLIES: set[str] = {
    "nunit.framework",
}

MONOBEHAVIOUR_HEADER_KEYS: set[str] = {
    "m_ObjectHideFlags",
    "m_CorrespondingSourceObject",
    "m_PrefabInstance",
    "m_PrefabAsset",
    "m_GameObject",
    "m_Enabled",
    "m_EditorHideFlags",
    "m_Script",
    "m_Name",
    "m_EditorClassIdentifier",
}


def deterministic_guid(rel_posix: str) -> str:
    return hashlib.md5(f"DudeChaosUnity:{rel_posix}".encode("utf-8")).hexdigest()


# Official Unity .cs.meta GUIDs from com.unity.render-pipelines.universal,
# com.unity.render-pipelines.core, and com.unity.ugui (TextMeshPro) when Library/PackageCache is absent.
PACKAGE_SCRIPT_WHITELIST: set[str] = {
    "bf2edee5c58d82540a51f03df9d42094",  # UniversalRenderPipelineAsset.cs
    "de640fe3d0db1804a85f9fc8f5cadab6",  # UniversalRendererData.cs
    "2ec995e51a6e251468d2a3fd8a686257",  # UniversalRenderPipelineGlobalSettings.cs
    "5485954d14dfb9a4c8ead8edb0ded5b1",  # VolumeProfile.cs
    "71c1514a6bd24e1e882cebbe1904ce04",  # TMP_FontAsset.cs
    "2705215ac5b84b70bacc50632be6e391",  # TMP_Settings.cs
    "84a92b25f83d49b9bc132d206b370281",  # TMP_SpriteAsset.cs
    "ab2114bdc8544297b417dfefe9f1e410",  # TMP_StyleSheet.cs
}


def strip_cs_comments_and_strings(src: str) -> str:
    # Remove block comments, line comments, and string/char literals while preserving line structure.
    pattern = re.compile(
        r'//[^\n]*|/\*[\s\S]*?\*/|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'',
        re.MULTILINE,
    )
    return pattern.sub(lambda m: "\n" * m.group(0).count("\n"), src)


def parse_meta_guid(meta_path: Path) -> str | None:
    try:
        text = meta_path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    m = re.search(r"^guid:\s*([0-9a-fA-F]{32})\b", text, re.MULTILINE)
    return m.group(1).lower() if m else None


def find_covering_asmdef(cs_path: Path, asmdef_by_dir: dict[Path, str]) -> str | None:
    cur = cs_path.parent
    assets_root = REPO_ROOT / "Assets"
    while True:
        if cur in asmdef_by_dir:
            return asmdef_by_dir[cur]
        if cur == assets_root or cur.parent == cur:
            break
        cur = cur.parent
    return None


def extract_cs_types_and_fields(cs_path: Path) -> tuple[str | None, list[str], dict[str, tuple[str | None, set[str]]]]:
    """Returns (namespace, declared_type_names, class_info_map[class_name -> (base_class, serializable_fields)])."""
    raw = cs_path.read_text(encoding="utf-8", errors="replace")
    clean = strip_cs_comments_and_strings(raw)

    ns_match = re.search(r"\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)", clean)
    namespace = ns_match.group(1) if ns_match else None

    type_decl_re = re.compile(
        r"\b(?:public|internal|private|protected)?\s*"
        r"(?:static\s+|abstract\s+|sealed\s+|partial\s+|readonly\s+)*"
        r"(class|struct|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)"
        r"(?:\s*:\s*([A-Za-z0-9_.,\s<>]+))?"
    )

    declared_types: list[str] = []
    class_info: dict[str, tuple[str | None, set[str]]] = {}

    matches = list(type_decl_re.finditer(clean))
    for idx, m in enumerate(matches):
        kind = m.group(1)
        tname = m.group(2)
        inheritance = m.group(3)
        declared_types.append(tname)

        if kind in ("class", "struct"):
            base_type = None
            if inheritance:
                first_base = inheritance.split(",")[0].strip()
                base_type = first_base.split(".")[-1].strip()
            # Slice body roughly up to next top-level type or EOF, tracking brace depth of this type.
            brace_start = clean.find("{", m.end())
            if brace_start != -1:
                depth = 0
                end_pos = len(clean)
                for pos in range(brace_start, len(clean)):
                    ch = clean[pos]
                    if ch == "{":
                        depth += 1
                    elif ch == "}":
                        depth -= 1
                        if depth == 0:
                            end_pos = pos
                            break
                body = clean[brace_start + 1 : end_pos]
                fields = extract_serializable_fields(body)
                class_info[tname] = (base_type, fields)

    return namespace, declared_types, class_info


def extract_serializable_fields(class_body: str) -> set[str]:
    """Extracts Unity-serializable field names declared directly inside class_body (depth 0)."""
    fields: set[str] = set()
    # Collapse nested {...} blocks (methods, properties, nested types, array initializers) at depth >= 1
    top_chars: list[str] = []
    depth = 0
    for ch in class_body:
        if ch == "{":
            depth += 1
            if depth == 1:
                top_chars.append("{}")
        elif ch == "}":
            depth = max(0, depth - 1)
        else:
            if depth == 0:
                top_chars.append(ch)
    flat = "".join(top_chars)

    # Split statements by ';'
    for stmt in flat.split(";"):
        s = stmt.strip()
        if not s:
            continue
        has_non_serialized = bool(re.search(r"\[\s*(?:System\.)?NonSerialized\s*\]", s))
        if has_non_serialized:
            continue
        has_serialize_field = bool(re.search(r"\[\s*SerializeField\b", s))
        # Strip attributes [...] first (attributes like [Tooltip(...)] contain parentheses)
        s_no_attr = re.sub(r"\[[^\]]*\]", " ", s).strip()
        # Remove '= initializer' before checking '(' so '= AnimationCurve.Linear(...)' or '= new Vector3(...)' is kept
        if "=" in s_no_attr:
            s_no_attr = s_no_attr.split("=", 1)[0].strip()
        if not s_no_attr or "(" in s_no_attr:
            continue
        if re.search(r"\b(?:const|static|readonly|event)\b", s_no_attr):
            continue
        is_public = bool(re.match(r"^public\b", s_no_attr))
        if not (is_public or has_serialize_field):
            continue
        # Remove visibility modifiers
        s_decl = re.sub(r"^(?:public|private|protected|internal)\s+", "", s_no_attr).strip()
        # Remaining should be '<Type> <fieldName>' (possibly with generics or [] array)
        m = re.match(r"^[A-Za-z_][A-Za-z0-9_<>,.\[\]\s]*?\s+([A-Za-z_][A-Za-z0-9_]*)$", s_decl)
        if m:
            fields.add(m.group(1))
    return fields


def extract_usings(cs_path: Path) -> list[str]:
    raw = cs_path.read_text(encoding="utf-8", errors="replace")
    clean = strip_cs_comments_and_strings(raw)
    usings: list[str] = []
    for m in re.finditer(r"^\s*using\s+(?:[A-Za-z_][A-Za-z0-9_]*\s*=\s*)?(DudeChaos\.[A-Za-z0-9_.]+)\s*;", clean, re.MULTILINE):
        usings.append(m.group(1))
    return usings


def is_finite_number(token: str) -> bool:
    t = token.strip()
    if not t:
        return False
    try:
        v = float(t)
    except ValueError:
        return False
    return math.isfinite(v)


def audit_asset_file(
    asset_path: Path,
    rel_posix: str,
    guid_to_cs: dict[str, Path],
    det_guid_to_cs: dict[str, Path],
    cs_class_fields: dict[Path, set[str]],
    has_package_cache: bool,
    findings: list[str],
) -> None:
    text = asset_path.read_text(encoding="utf-8", errors="replace")

    # 1. ASSET_VECTOR: every {x:..., y:...[, z:...][, w:...]} must have finite numeric components
    vec_re = re.compile(
        r"\{\s*x:\s*([^,}]+),\s*y:\s*([^,}]+)(?:,\s*z:\s*([^,}]+))?(?:,\s*w:\s*([^,}]+))?\s*\}"
    )
    for lineno, line in enumerate(text.splitlines(), start=1):
        for vm in vec_re.finditer(line):
            comps = [c for c in vm.groups() if c is not None]
            if not all(is_finite_number(c) for c in comps):
                findings.append(f"ASSET_VECTOR: {rel_posix}:{lineno}: non-numeric vector {vm.group(0)}")

    # Check AnimationCurve keyframes for Landmine #24 (string in float field)
    in_curve = False
    for lineno, line in enumerate(text.splitlines(), start=1):
        stripped = line.strip()
        if stripped == "m_Curve:":
            in_curve = True
            continue
        if in_curve:
            if re.match(r"^\s{2,4}m_(?:PreInfinity|PostInfinity|RotationOrder):", line) or re.match(r"^\s{2}[A-Za-z_]", line):
                in_curve = False
            else:
                km = re.match(r"^\s*(?:-\s*)?(time|value|inSlope|outSlope|inWeight|outWeight):\s*(.*)$", line)
                if km and not is_finite_number(km.group(2)):
                    findings.append(f"ASSET_VECTOR: {rel_posix}:{lineno}: non-numeric curve {km.group(1)}={km.group(2)!r}")

    # 2. Extract m_Script GUID and MonoBehaviour top-level fields
    script_match = re.search(
        r"^\s*m_Script:\s*\{[^}]*\bguid:\s*([0-9a-fA-F]{32})\b[^}]*\}",
        text,
        re.MULTILINE,
    )
    if not script_match:
        findings.append(f"ASSET_SCRIPT_GUID: {rel_posix}: missing m_Script guid")
        return

    sguid = script_match.group(1).lower()
    cs_target = guid_to_cs.get(sguid)
    if cs_target is None:
        if not has_package_cache and sguid in PACKAGE_SCRIPT_WHITELIST:
            return
        findings.append(f"ASSET_SCRIPT_GUID: {rel_posix}: m_Script guid {sguid} resolves to no .cs.meta")
        # Still check field names if deterministic GUID matches a known .cs file
        cs_target = det_guid_to_cs.get(sguid)
        if cs_target is None:
            return

    allowed_fields = cs_class_fields.get(cs_target)
    if allowed_fields is None:
        return

    # Parse top-level serialized properties under MonoBehaviour: (2-space indent)
    in_mb = False
    for lineno, line in enumerate(text.splitlines(), start=1):
        if line.startswith("MonoBehaviour:"):
            in_mb = True
            continue
        if not in_mb:
            continue
        if line.startswith("--- "):
            in_mb = False
            continue
        fm = re.match(r"^  ([A-Za-z_][A-Za-z0-9_]*):", line)
        if fm:
            fname = fm.group(1)
            if fname in MONOBEHAVIOUR_HEADER_KEYS:
                continue
            if fname not in allowed_fields:
                findings.append(
                    f"ASSET_FIELD_MISMATCH: {rel_posix}:{lineno}: field '{fname}' not found on {cs_target.stem}"
                )


def main() -> int:
    findings: list[str] = []
    assets_dir = REPO_ROOT / "Assets"

    # 1. ProjectVersion.txt
    pv_path = REPO_ROOT / "ProjectSettings" / "ProjectVersion.txt"
    if not pv_path.is_file():
        findings.append("PROJECT_VERSION: ProjectSettings/ProjectVersion.txt missing")
    else:
        pv_text = pv_path.read_text(encoding="utf-8", errors="replace")
        if "m_EditorVersion: 6000.3.28f1" not in pv_text:
            findings.append(f"PROJECT_VERSION: expected 6000.3.28f1 in ProjectVersion.txt, got: {pv_text.splitlines()[0] if pv_text else 'empty'}")

    # 2. Packages/manifest.json
    manifest_path = REPO_ROOT / "Packages" / "manifest.json"
    manifest_deps: set[str] = set()
    if not manifest_path.is_file():
        findings.append("PACKAGE_NOT_IN_MANIFEST: Packages/manifest.json missing")
    else:
        try:
            manifest_data = json.loads(manifest_path.read_text(encoding="utf-8"))
            manifest_deps = set((manifest_data.get("dependencies") or {}).keys())
        except json.JSONDecodeError as exc:
            findings.append(f"PACKAGE_NOT_IN_MANIFEST: invalid JSON in Packages/manifest.json: {exc}")

    # 3. Parse all *.asmdef under Assets/
    asmdef_files = sorted(assets_dir.rglob("*.asmdef"))
    asm_by_name: dict[str, Path] = {}
    asmdef_by_dir: dict[Path, str] = {}
    asm_refs: dict[str, list[str]] = {}
    asm_guid_to_name: dict[str, str] = {}
    ns_to_asms: dict[str, set[str]] = {}

    for af in asmdef_files:
        rel = af.relative_to(REPO_ROOT).as_posix()
        try:
            data = json.loads(af.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            findings.append(f"ASMDEF_PARSE: {rel}: {exc}")
            continue
        name = data.get("name")
        if not name or not isinstance(name, str):
            findings.append(f"ASMDEF_PARSE: {rel}: missing string 'name'")
            continue
        if name in asm_by_name:
            other = asm_by_name[name].relative_to(REPO_ROOT).as_posix()
            findings.append(f"ASMDEF_DUPLICATE: '{name}' declared in both {other} and {rel}")
        else:
            asm_by_name[name] = af
        asmdef_by_dir[af.parent] = name
        refs = data.get("references") or []
        asm_refs[name] = [str(r) for r in refs]
        root_ns = data.get("rootNamespace")
        if isinstance(root_ns, str) and root_ns:
            ns_to_asms.setdefault(root_ns, set()).add(name)

        meta_p = Path(f"{af}.meta")
        if meta_p.is_file():
            aguid = parse_meta_guid(meta_p)
            if aguid:
                asm_guid_to_name[aguid] = name

    # Resolve any GUID:xxxx references
    resolved_refs: dict[str, list[str]] = {}
    for asm_name, raw_refs in asm_refs.items():
        rlist: list[str] = []
        for r in raw_refs:
            if r.startswith("GUID:"):
                g = r[5:].strip().lower()
                if g in asm_guid_to_name:
                    rlist.append(asm_guid_to_name[g])
                else:
                    findings.append(f"ASMDEF_DANGLING_REF: {asm_name} references unresolved {r}")
            else:
                rlist.append(r)
        resolved_refs[asm_name] = rlist

    # Check dangling references & manifest packages
    for asm_name, rlist in resolved_refs.items():
        for r in rlist:
            if r in asm_by_name or r in BUILTIN_UNITY_ASSEMBLIES:
                continue
            if r in UNITY_PACKAGE_ASM_TO_MANIFEST:
                pkgs = UNITY_PACKAGE_ASM_TO_MANIFEST[r]
                if not any(p in manifest_deps for p in pkgs):
                    findings.append(
                        f"PACKAGE_NOT_IN_MANIFEST: {asm_name} references '{r}', missing {'/'.join(pkgs)} in manifest.json"
                    )
            else:
                findings.append(f"ASMDEF_DANGLING_REF: {asm_name} references unknown assembly '{r}'")

    # 4. DFS cycle check on project assemblies
    visit_state: dict[str, int] = {}  # 0=unvisited, 1=visiting, 2=done
    stack: list[str] = []

    def dfs(node: str) -> None:
        visit_state[node] = 1
        stack.append(node)
        for nxt in resolved_refs.get(node, []):
            if nxt not in asm_by_name:
                continue
            st = visit_state.get(nxt, 0)
            if st == 1:
                idx = stack.index(nxt)
                cycle = " -> ".join(stack[idx:] + [nxt])
                findings.append(f"ASMDEF_CYCLE: {cycle}")
            elif st == 0:
                dfs(nxt)
        stack.pop()
        visit_state[node] = 2

    for asm_name in sorted(asm_by_name):
        if visit_state.get(asm_name, 0) == 0:
            dfs(asm_name)

    # 5. Scan all *.cs under Assets/
    cs_files = sorted(assets_dir.rglob("*.cs"))
    cs_to_asm: dict[Path, str] = {}
    total_types = 0
    type_locations: dict[tuple[str, str], Path] = {}
    all_class_defs: dict[str, tuple[str | None, set[str]]] = {}
    cs_primary_class: dict[Path, str] = {}
    det_guid_to_cs: dict[str, Path] = {}

    for csf in cs_files:
        rel_posix = csf.relative_to(REPO_ROOT).as_posix()
        det_guid_to_cs[deterministic_guid(rel_posix)] = csf
        cov = find_covering_asmdef(csf, asmdef_by_dir)
        if cov is None:
            findings.append(f"SCRIPT_UNCOVERED: {rel_posix} is not covered by any .asmdef")
        else:
            cs_to_asm[csf] = cov

        ns, dtypes, cinfo = extract_cs_types_and_fields(csf)
        total_types += len(dtypes)
        if ns and cov:
            ns_to_asms.setdefault(ns, set()).add(cov)

        for tname in dtypes:
            key = (ns or "", tname)
            if key in type_locations and type_locations[key] != csf:
                prev = type_locations[key].relative_to(REPO_ROOT).as_posix()
                findings.append(f"DUPLICATE_TYPE: '{ns + '.' if ns else ''}{tname}' in {prev} and {rel_posix}")
            else:
                type_locations[key] = csf

        for cname, info in cinfo.items():
            all_class_defs[cname] = info
        if csf.stem in cinfo:
            cs_primary_class[csf] = csf.stem
        elif cinfo:
            cs_primary_class[csf] = next(iter(cinfo))

    # Resolve inherited serializable fields across project ScriptableObjects
    def resolved_fields_for(cname: str, seen: set[str] | None = None) -> set[str]:
        if seen is None:
            seen = set()
        if cname in seen or cname not in all_class_defs:
            return set()
        seen.add(cname)
        base_name, own = all_class_defs[cname]
        merged = set(own)
        if base_name and base_name in all_class_defs:
            merged |= resolved_fields_for(base_name, seen)
        return merged

    cs_class_fields: dict[Path, set[str]] = {}
    for csf, cname in cs_primary_class.items():
        cs_class_fields[csf] = resolved_fields_for(cname)

    # 6. USING_NOT_REFERENCED
    for csf, asm_name in cs_to_asm.items():
        rel_posix = csf.relative_to(REPO_ROOT).as_posix()
        refs_set = set(resolved_refs.get(asm_name, []))
        for used_ns in extract_usings(csf):
            owners = ns_to_asms.get(used_ns)
            if not owners:
                findings.append(f"USING_NOT_REFERENCED: {rel_posix} ({asm_name}) uses unknown namespace '{used_ns}'")
                continue
            if asm_name in owners:
                continue
            if not any(o in refs_set for o in owners):
                findings.append(
                    f"USING_NOT_REFERENCED: {rel_posix} ({asm_name}) uses '{used_ns}' (owned by {'/'.join(sorted(owners))}) without assembly reference"
                )

    # 7. Scan *.meta files under Assets/ (and Library/PackageCache when present)
    guid_seen: dict[str, str] = {}
    guid_to_cs: dict[str, Path] = {}

    for meta_file in sorted(assets_dir.rglob("*.meta")):
        rel_meta = meta_file.relative_to(REPO_ROOT).as_posix()
        g = parse_meta_guid(meta_file)
        if not g:
            findings.append(f"META_INVALID: {rel_meta}: missing 32-hex guid")
            continue
        if g in guid_seen:
            findings.append(f"DUPLICATE_GUID: {g} shared by {guid_seen[g]} and {rel_meta}")
        else:
            guid_seen[g] = rel_meta

        if meta_file.name.endswith(".cs.meta"):
            cs_target = Path(str(meta_file)[:-5])
            if cs_target.is_file():
                guid_to_cs[g] = cs_target

    pkg_cache = REPO_ROOT / "Library" / "PackageCache"
    has_package_cache = pkg_cache.is_dir()
    if has_package_cache:
        for meta_file in pkg_cache.rglob("*.cs.meta"):
            g = parse_meta_guid(meta_file)
            if g:
                cs_target = Path(str(meta_file)[:-5])
                if cs_target.is_file():
                    guid_to_cs[g] = cs_target

    # 8. Audit all *.asset files under Assets/
    asset_files = sorted(assets_dir.rglob("*.asset"))
    for af in asset_files:
        rel_posix = af.relative_to(REPO_ROOT).as_posix()
        audit_asset_file(
            af,
            rel_posix,
            guid_to_cs,
            det_guid_to_cs,
            cs_class_fields,
            has_package_cache,
            findings,
        )

    # 9. Audit all *.unity scene files under Assets/Scenes/ (vectors, MonoBehaviour m_Script GUIDs & field names)
    vec_re = re.compile(
        r"\{\s*x:\s*([^,}]+),\s*y:\s*([^,}]+)(?:,\s*z:\s*([^,}]+))?(?:,\s*w:\s*([^,}]+))?\s*\}"
    )
    for sf in sorted(assets_dir.rglob("*.unity")):
        rel_posix = sf.relative_to(REPO_ROOT).as_posix()
        stext = sf.read_text(encoding="utf-8", errors="replace")
        for lineno, line in enumerate(stext.splitlines(), start=1):
            for vm in vec_re.finditer(line):
                comps = [c for c in vm.groups() if c is not None]
                if not all(is_finite_number(c) for c in comps):
                    findings.append(f"SCENE_VECTOR: {rel_posix}:{lineno}: non-numeric vector {vm.group(0)}")

        # Split scene YAML by '--- !u!' documents and validate every MonoBehaviour (!u!114)
        for doc in stext.split("--- !u!"):
            if not doc.startswith("114 "):
                continue
            sm = re.search(
                r"^\s*m_Script:\s*\{[^}]*\bguid:\s*([0-9a-fA-F]{32})\b[^}]*\}",
                doc,
                re.MULTILINE,
            )
            if not sm:
                findings.append(f"SCENE_SCRIPT_GUID: {rel_posix}: MonoBehaviour missing m_Script guid")
                continue
            sguid = sm.group(1).lower()
            cs_target = guid_to_cs.get(sguid) or det_guid_to_cs.get(sguid)
            if cs_target is None:
                if not has_package_cache and sguid in PACKAGE_SCRIPT_WHITELIST:
                    continue
                findings.append(f"SCENE_SCRIPT_GUID: {rel_posix}: m_Script guid {sguid} resolves to no .cs.meta")
                continue
            allowed = cs_class_fields.get(cs_target, set())
            for dline in doc.splitlines():
                fm = re.match(r"^  ([A-Za-z_][A-Za-z0-9_]*):", dline)
                if fm:
                    fname = fm.group(1)
                    if fname in MONOBEHAVIOUR_HEADER_KEYS:
                        continue
                    if fname not in allowed:
                        findings.append(
                            f"SCENE_FIELD_MISMATCH: {rel_posix}: field '{fname}' not found on {cs_target.stem}"
                        )

    # 10. Verify ProjectSettings/EditorBuildSettings.asset scene paths & GUIDs
    ebs_path = REPO_ROOT / "ProjectSettings" / "EditorBuildSettings.asset"
    if ebs_path.is_file():
        ebs_text = ebs_path.read_text(encoding="utf-8", errors="replace")
        for m in re.finditer(r"path:\s*(Assets/[^\s]+)\s*\n\s*guid:\s*([0-9a-fA-F]{32})", ebs_text):
            spath, sguid = m.group(1), m.group(2).lower()
            sfile = REPO_ROOT / spath
            smeta = Path(f"{sfile}.meta")
            if not sfile.is_file():
                findings.append(f"BUILD_SETTINGS_SCENE: missing scene file {spath}")
            elif smeta.is_file():
                actual_guid = parse_meta_guid(smeta)
                if actual_guid != sguid:
                    findings.append(
                        f"BUILD_SETTINGS_SCENE: {spath} guid {sguid} != meta guid {actual_guid}"
                    )

    for f in findings:
        print(f"FINDING: {f}")

    summary = f"{len(findings)} findings ({len(asm_by_name)} assemblies / {total_types} types / {len(asset_files)} assets)"
    print(summary)
    return 0 if len(findings) == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
