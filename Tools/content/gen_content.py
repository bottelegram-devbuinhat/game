#!/usr/bin/env python3
"""Deterministic Unity YAML generator for all 196 Dude Chaos Wars assets (§1.6, §1.9, §2.1-2.6).

Uses named fields exclusively (Landmine #25) and numeric vectors/curves exclusively
(Landmines #1 & #24). Computes deterministic GUIDs with:
  md5("DudeChaosUnity:" + posix_rel_path_from_repo_root)
"""

from __future__ import annotations

import dataclasses
import hashlib
import os
import re
import struct
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
sys.path.insert(0, str(SCRIPT_DIR))

from story_lines import MISSION_DIALOGUE, validate_story_lines  # noqa: E402


def unity_guid(posix_rel_path: str) -> str:
    norm = posix_rel_path.replace("\\", "/").lstrip("/")
    return hashlib.md5(f"DudeChaosUnity:{norm}".encode("utf-8")).hexdigest()


def package_guid(package_type_name: str) -> str:
    return hashlib.md5(f"UnityPackage:{package_type_name}".encode("utf-8")).hexdigest()


SCRIPT_GUIDS: dict[str, str] = {
    "WeaponData": unity_guid("Assets/Scripts/Data/WeaponData.cs"),
    "VehicleData": unity_guid("Assets/Scripts/Data/VehicleData.cs"),
    "EquipmentData": unity_guid("Assets/Scripts/Data/EquipmentData.cs"),
    "ShopItem": unity_guid("Assets/Scripts/Data/ShopItem.cs"),
    "MissionData": unity_guid("Assets/Scripts/Data/MissionData.cs"),
    "DistrictDefinition": unity_guid("Assets/Scripts/City/DistrictDefinition.cs"),
    "MovementSettings": unity_guid("Assets/Scripts/Data/MovementSettings.cs"),
    "RagdollSettings": unity_guid("Assets/Scripts/Data/RagdollSettings.cs"),
    "CombatSettings": unity_guid("Assets/Scripts/Data/CombatSettings.cs"),
    "VehicleSettings": unity_guid("Assets/Scripts/Data/VehicleSettings.cs"),
    "WantedSettings": unity_guid("Assets/Scripts/Data/WantedSettings.cs"),
    "QualitySettingsPreset": unity_guid("Assets/Scripts/Data/QualitySettingsPreset.cs"),
    "CameraShakeProfile": unity_guid("Assets/Scripts/Data/CameraShakeProfile.cs"),
    "ImpactMaterialData": unity_guid("Assets/Scripts/Data/ImpactMaterialData.cs"),
    "EmoteData": unity_guid("Assets/Scripts/Data/EmoteData.cs"),
    # URP / Core RP package ScriptableObjects (§1.9.1 & §1.9.6)
    "UniversalRenderPipelineAsset": package_guid("UniversalRenderPipelineAsset"),
    "UniversalRendererData": package_guid("UniversalRendererData"),
    "UniversalRenderPipelineGlobalSettings": package_guid("UniversalRenderPipelineGlobalSettings"),
    "VolumeProfile": package_guid("VolumeProfile"),
}

WEAPON_CLASSES = {
    "Pistol": 0,
    "SMG": 1,
    "Rifle": 2,
    "Shotgun": 3,
    "Sniper": 4,
    "Heavy": 5,
    "Melee": 6,
}

HANDLING_PROFILES = {
    "Sports": 0,
    "Muscle": 1,
    "Truck": 2,
    "Bike": 3,
}

ARMOUR_CLASSES = {
    "Unarmoured": 0,
    "Light": 1,
    "Heavy": 2,
    "Air": 3,
}

EQUIPMENT_KINDS = {
    "Attachment": 0,
    "Consumable": 1,
    "Support": 2,
    "Upgrade": 3,
    "Ammo": 4,
}

ATTACHMENT_SLOTS = {
    "-": 0,
    "None": 0,
    "Optic": 1,
    "Muzzle": 2,
    "Underbarrel": 3,
    "Magazine": 4,
    "Stock": 5,
    "Laser": 6,
}

SHOP_ITEM_TYPES = {
    "Weapon": 0,
    "Skin": 1,
    "Vehicle": 2,
    "Emote": 3,
}

DISTRICT_KINDS = {
    "Downtown": 0,
    "Slums": 1,
    "Market": 2,
    "Restaurant": 3,
    "Hotel": 4,
    "Showroom": 5,
    "Residential": 6,
    "Port": 7,
    "Airport": 8,
    "Farm": 9,
    "Military": 10,
    "Secret": 11,
}


def fmt_num(val: float | int) -> str:
    if isinstance(val, int):
        return str(val)
    if float(val).is_integer():
        return str(int(val))
    return f"{val:.6g}"


def yaml_str(text: str) -> str:
    if text == "":
        return '""'
    escaped = text.replace("\\", "\\\\").replace('"', '\\"')
    return f'"{escaped}"'


def encode_int_array_hex(values: list[int]) -> str:
    if not values:
        return ""
    return "".join(struct.pack("<i", v).hex() for v in values)


def monobehaviour_header(asset_name: str, script_type: str) -> str:
    script_guid = SCRIPT_GUIDS[script_type]
    return (
        "%YAML 1.1\n"
        "%TAG !u! tag:unity3d.com,2011:\n"
        "--- !u!114 &11400000\n"
        "MonoBehaviour:\n"
        "  m_ObjectHideFlags: 0\n"
        "  m_CorrespondingSourceObject: {fileID: 0}\n"
        "  m_PrefabInstance: {fileID: 0}\n"
        "  m_PrefabAsset: {fileID: 0}\n"
        "  m_GameObject: {fileID: 0}\n"
        "  m_Enabled: 1\n"
        "  m_EditorHideFlags: 0\n"
        f"  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}\n"
        f"  m_Name: {asset_name}\n"
        "  m_EditorClassIdentifier: \n"
    )


def animation_curve_yaml(indent: str, y0: float, y1: float) -> str:
    slope = y1 - y0
    return (
        f"{indent}serializedVersion: 2\n"
        f"{indent}m_Curve:\n"
        f"{indent}- serializedVersion: 3\n"
        f"{indent}  time: 0\n"
        f"{indent}  value: {fmt_num(y0)}\n"
        f"{indent}  inSlope: {fmt_num(slope)}\n"
        f"{indent}  outSlope: {fmt_num(slope)}\n"
        f"{indent}  tangentMode: 0\n"
        f"{indent}  weightedMode: 0\n"
        f"{indent}  inWeight: 0\n"
        f"{indent}  outWeight: 0\n"
        f"{indent}- serializedVersion: 3\n"
        f"{indent}  time: 1\n"
        f"{indent}  value: {fmt_num(y1)}\n"
        f"{indent}  inSlope: {fmt_num(slope)}\n"
        f"{indent}  outSlope: {fmt_num(slope)}\n"
        f"{indent}  tangentMode: 0\n"
        f"{indent}  weightedMode: 0\n"
        f"{indent}  inWeight: 0\n"
        f"{indent}  outWeight: 0\n"
        f"{indent}m_PreInfinity: 2\n"
        f"{indent}m_PostInfinity: 2\n"
        f"{indent}m_RotationOrder: 4\n"
    )


def write_asset_and_meta(rel_path: str, content: str) -> None:
    norm_rel = rel_path.replace("\\", "/")
    full_path = REPO_ROOT / norm_rel
    full_path.parent.mkdir(parents=True, exist_ok=True)
    full_path.write_text(content, encoding="utf-8", newline="\n")

    guid = unity_guid(norm_rel)
    meta = (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "NativeFormatImporter:\n"
        "  externalObjects: {}\n"
        "  mainObjectFileID: 11400000\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )
    meta_path = REPO_ROOT / f"{norm_rel}.meta"
    meta_path.write_text(meta, encoding="utf-8", newline="\n")


def write_folder_meta(rel_dir: str) -> None:
    norm_rel = rel_dir.replace("\\", "/").rstrip("/")
    dir_path = REPO_ROOT / norm_rel
    dir_path.mkdir(parents=True, exist_ok=True)
    guid = unity_guid(norm_rel)
    meta = (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "folderAsset: yes\n"
        "DefaultImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )
    (REPO_ROOT / f"{norm_rel}.meta").write_text(meta, encoding="utf-8", newline="\n")


def slugify_title(name: str) -> str:
    cleaned = name.replace(".", "").replace("-", "_")
    cleaned = re.sub(r"[^A-Za-z0-9_]+", "_", cleaned).strip("_")
    return re.sub(r"_+", "_", cleaned)


@dataclasses.dataclass(frozen=True)
class WeaponSpec:
    display_name: str
    weapon_class: str
    damage: float
    fire_rate: float
    magazine_size: int
    range_m: float
    mechanic: str
    is_automatic: bool
    charge_seconds: float
    burst_count: int
    pellets: int
    reload_time: float
    spread: float
    ragdoll_multiplier: float
    muzzle_energy_joules: float
    projectile_speed: float = 0.0
    self_damage: float = 0.0
    heat_per_shot: float = 0.0


WEAPONS: tuple[WeaponSpec, ...] = (
    WeaponSpec("Anti-Tank Rifle", "Sniper", 300, 2.8, 1, 350, "Bolt", False, 0, 1, 1, 3.2, 0.15, 2.6, 32000),
    WeaponSpec("C4 Charge", "Heavy", 300, 1.8, 1, 6, "Throwable", False, 0.6, 1, 1, 2.5, 0.5, 2.8, 60000, 14, 105),
    WeaponSpec("Railgun", "Heavy", 260, 2.4, 1, 300, "Charge", False, 1.8, 1, 1, 2.8, 0.1, 2.5, 45000, 0, 0, 0.25),
    WeaponSpec("Rocket Launcher", "Heavy", 200, 1.6, 1, 120, "", False, 0, 1, 1, 3.0, 0.8, 2.4, 22000, 55, 70),
    WeaponSpec("Mortar Tube", "Heavy", 190, 2.2, 1, 180, "Projectile", False, 0.9, 1, 1, 3.0, 1.4, 2.3, 18000, 42, 66.5),
    WeaponSpec("Frag Grenade", "Heavy", 160, 1.6, 1, 22, "Throwable", False, 0.5, 1, 1, 1.8, 0.8, 2.2, 12000, 22, 56),
    WeaponSpec("Anti-Materiel", "Sniper", 120, 1.4, 5, 400, "", False, 0, 1, 1, 2.8, 0.18, 2.0, 17296),
    WeaponSpec("Grenade Launcher", "Heavy", 120, 1.0, 6, 80, "", False, 0, 1, 1, 2.8, 1.1, 2.0, 9000, 48, 42),
    WeaponSpec("Slug Shotgun", "Shotgun", 96, 0.8, 5, 35, "Pump", False, 0, 1, 1, 2.4, 0.9, 1.9, 3376),
    WeaponSpec("Charge Rifle", "Rifle", 95, 1.2, 5, 200, "Charge", False, 1.1, 1, 1, 2.2, 0.25, 1.7, 12000, 0, 0, 0.2),
    WeaponSpec("Bolt Sniper", "Sniper", 95, 1.15, 5, 300, "", False, 0, 1, 1, 2.5, 0.2, 1.7, 6800),
    WeaponSpec("Pump Shotgun", "Shotgun", 88, 0.85, 8, 22, "", False, 0, 1, 8, 2.6, 4.5, 1.8, 3100),
    WeaponSpec("Semi Sniper", "Sniper", 78, 0.65, 10, 250, "", False, 0, 1, 1, 2.2, 0.35, 1.5, 3592),
    WeaponSpec("Arc Caster", "Heavy", 70, 0.9, 8, 60, "Charge", False, 0.7, 1, 1, 2.3, 0.6, 1.6, 4500, 0, 0, 0.18),
    WeaponSpec("Molotov", "Heavy", 70, 1.4, 1, 18, "Throwable", False, 0.4, 1, 1, 1.6, 1.0, 1.4, 4500, 20, 24.5),
    WeaponSpec("Auto Shotgun", "Shotgun", 62, 0.45, 10, 20, "", False, 0, 1, 8, 2.5, 5.0, 1.6, 2850),
    WeaponSpec("Sledgehammer", "Melee", 62, 0.75, 0, 2.2, "", False, 0, 1, 1, 0.0, 0.0, 2.2, 0),
    WeaponSpec("Revolver .44", "Pistol", 45, 0.55, 6, 55, "", False, 0, 1, 1, 2.1, 0.8, 1.5, 750),
    WeaponSpec("Double Barrel", "Shotgun", 44, 0.35, 2, 16, "Semi", False, 0, 1, 8, 1.9, 5.5, 1.9, 2600),
    WeaponSpec("Katana", "Melee", 40, 0.38, 0, 2.4, "", False, 0, 1, 1, 0.0, 0.0, 1.4, 0),
    WeaponSpec("Airburst Rifle", "Rifle", 40, 1.1, 12, 150, "Burst", False, 0, 2, 1, 2.4, 0.9, 1.5, 9000),
    WeaponSpec("DMR", "Rifle", 38, 0.28, 20, 180, "", False, 0, 1, 1, 2.1, 0.55, 1.3, 3592),
    WeaponSpec("Battle Rifle", "Rifle", 34, 0.12, 25, 150, "", True, 0, 1, 1, 2.2, 1.1, 1.3, 3200),
    WeaponSpec("Crowbar", "Melee", 34, 0.42, 0, 2.2, "", False, 0, 1, 1, 0.0, 0.0, 1.5, 0),
    WeaponSpec("Laser Rifle", "Rifle", 34, 0.09, 60, 90, "Beam", True, 0, 0, 1, 2.0, 0.2, 1.1, 3200, 0, 0, 0.35),
    WeaponSpec("Heavy Pistol", "Pistol", 32, 0.28, 10, 45, "", False, 0, 1, 1, 1.7, 1.0, 1.25, 500),
    WeaponSpec("AK Rifle", "Rifle", 32, 0.11, 30, 120, "", True, 0, 1, 1, 2.1, 1.35, 1.25, 2045),
    WeaponSpec("Sawn-off", "Shotgun", 30, 0.7, 2, 18, "", False, 0, 1, 8, 1.8, 6.2, 1.8, 2300),
    WeaponSpec("LMG", "Heavy", 26, 0.09, 100, 90, "", True, 0, 1, 1, 3.4, 1.8, 1.3, 2045, 0, 0, 0.02),
    WeaponSpec("Knife", "Melee", 26, 0.32, 0, 1.8, "", False, 0, 1, 1, 0.0, 0.0, 1.0, 0),
    WeaponSpec("Carbine", "Rifle", 26, 0.09, 30, 130, "", True, 0, 1, 1, 1.9, 1.15, 1.15, 1793),
    WeaponSpec("Chainsaw", "Melee", 24, 0.12, 0, 2.0, "Beam", True, 0, 0, 1, 0.0, 0.0, 1.3, 0, 0, 0, 0.25),
    WeaponSpec("Burst Carbine", "Rifle", 22, 0.85, 30, 120, "Burst", False, 0, 3, 1, 1.9, 1.0, 1.1, 1650),
    WeaponSpec("9mm Pistol", "Pistol", 22, 0.22, 12, 60, "", False, 0, 1, 1, 1.5, 1.2, 1.0, 482),
    WeaponSpec("Nailgun", "Heavy", 20, 0.07, 50, 40, "Projectile", True, 0, 1, 1, 2.0, 1.6, 1.1, 1950, 75, 0),
    WeaponSpec("Flare Gun", "Pistol", 18, 1.1, 1, 45, "Projectile", False, 0, 1, 1, 1.8, 1.4, 1.1, 450, 40, 6.3),
    WeaponSpec("PDW", "SMG", 18, 0.075, 35, 60, "", True, 0, 1, 1, 1.8, 1.6, 1.0, 620),
    WeaponSpec("Baseball Bat", "Melee", 18, 0.45, 0, 2.5, "", False, 0, 1, 1, 0.0, 0.0, 1.7, 0),
    WeaponSpec("Compact Pistol", "Pistol", 16, 0.16, 15, 35, "", False, 0, 1, 1, 1.4, 1.35, 0.95, 420),
    WeaponSpec("Combat Shotgun", "Shotgun", 16, 0.22, 8, 30, "", True, 0, 1, 8, 2.3, 4.2, 1.5, 2100),
    WeaponSpec("Flamethrower", "Heavy", 16, 0.06, 120, 14, "Projectile", True, 0, 1, 1, 3.0, 2.8, 1.0, 1850, 24, 0, 0.04),
    WeaponSpec("Vector SMG", "SMG", 14, 0.075, 30, 45, "", True, 0, 1, 1, 1.6, 1.7, 0.95, 500),
    WeaponSpec("Stun Baton", "Melee", 14, 0.55, 0, 2.0, "Semi", False, 0, 1, 1, 0.0, 0.0, 1.3, 0),
    WeaponSpec("Minigun", "Heavy", 14, 0.045, 150, 90, "", True, 0, 1, 1, 3.8, 2.4, 1.2, 1793, 0, 0, 0.04),
    WeaponSpec("Machine Pistol", "Pistol", 12, 0.07, 18, 30, "", True, 0, 1, 1, 1.5, 1.9, 0.9, 380),
    WeaponSpec("Uzi", "SMG", 11, 0.065, 25, 35, "", True, 0, 1, 1, 1.6, 2.0, 0.9, 440),
    WeaponSpec("Micro SMG", "SMG", 9, 0.05, 20, 30, "", True, 0, 1, 1, 1.4, 2.2, 0.85, 360),
    WeaponSpec("Taser", "Pistol", 6, 0.9, 2, 9, "Semi", False, 0, 1, 1, 1.7, 0.8, 1.8, 15),
)


@dataclasses.dataclass(frozen=True)
class VehicleSpec:
    vehicle_name: str
    handling_profile: str
    armour_class: str
    max_health: float
    seats: int
    com_y: float
    max_speed: float
    acceleration: float
    brake_force: float
    steer_angle: float


VEHICLES: tuple[VehicleSpec, ...] = (
    VehicleSpec("Armoured Carrier", "Truck", "Heavy", 4800, 6, -0.55, 26, 8, 30, 24),
    VehicleSpec("Ambulance", "Truck", "Unarmoured", 1800, 4, -0.50, 28, 9, 30, 27),
    VehicleSpec("Anti-Air Truck", "Truck", "Light", 3600, 4, -0.50, 24, 8, 30, 26),
    VehicleSpec("Sport Bike", "Bike", "Unarmoured", 400, 2, -0.25, 45, 22, 32, 25),
    VehicleSpec("Street Bike", "Bike", "Unarmoured", 450, 2, -0.25, 42, 20, 30, 26),
    VehicleSpec("City Bus", "Truck", "Unarmoured", 2200, 8, -0.50, 22, 7, 26, 22),
    VehicleSpec("Helicopter", "Bike", "Air", 2600, 3, -0.25, 46, 14, 30, 30),
    VehicleSpec("Light Jet", "Bike", "Air", 1800, 1, -0.25, 55, 22, 32, 28),
    VehicleSpec("Rocket Truck", "Truck", "Light", 3200, 3, -0.50, 22, 7, 28, 22),
    VehicleSpec("Monster Truck", "Truck", "Unarmoured", 2000, 4, -0.50, 30, 12, 30, 34),
    VehicleSpec("Muscle GT", "Muscle", "Unarmoured", 1000, 2, -0.45, 36, 16, 24, 34),
    VehicleSpec("Police Cruiser", "Sports", "Unarmoured", 1100, 4, -0.35, 38, 16, 30, 36),
    VehicleSpec("Sedan", "Sports", "Unarmoured", 900, 4, -0.35, 33, 14, 26, 38),
    VehicleSpec("Sport Coupe", "Sports", "Unarmoured", 850, 2, -0.30, 39, 17, 28, 36),
    VehicleSpec("Tank", "Truck", "Heavy", 12000, 3, -0.50, 18, 5, 28, 18),
    VehicleSpec("Taxi", "Sports", "Unarmoured", 850, 4, -0.35, 30, 13, 26, 36),
    VehicleSpec("Pickup Truck", "Truck", "Unarmoured", 1400, 4, -0.50, 25, 10, 30, 30),
    VehicleSpec("Cargo Van", "Truck", "Unarmoured", 1600, 6, -0.50, 22, 8, 28, 28),
)


@dataclasses.dataclass(frozen=True)
class EquipmentSpec:
    equipment_id: str
    display_name: str
    kind: str
    slot: str
    price: int
    unlock_level: int
    value_a: float
    value_b: float
    value_c: float
    value_d: float


EQUIPMENT: tuple[EquipmentSpec, ...] = (
    EquipmentSpec("eq_ammo_12g", "12 Gauge Shells", "Ammo", "-", 240, 1, 1.25, 1, 0, 0),
    EquipmentSpec("eq_ammo_338", "338 Rounds", "Ammo", "-", 600, 3, 1.6, 1, 0, 0),
    EquipmentSpec("eq_ammo_45", "45 ACP Rounds", "Ammo", "-", 180, 1, 1.15, 1, 0, 0),
    EquipmentSpec("eq_ammo_556", "5.56 Rounds", "Ammo", "-", 260, 2, 1.2, 1, 0, 0),
    EquipmentSpec("eq_ammo_762", "7.62 Rounds", "Ammo", "-", 320, 2, 1.35, 1, 0, 0),
    EquipmentSpec("eq_ammo_9mm", "9mm Rounds", "Ammo", "-", 120, 1, 1, 1, 0, 0),
    EquipmentSpec("eq_ammo_ap", "AP Rounds", "Ammo", "-", 800, 3, 1.3, 2, 0, 0),
    EquipmentSpec("eq_ammo_hp", "Hollow Point", "Ammo", "-", 700, 2, 1.45, 0.9, 0, 0),
    EquipmentSpec("eq_ammo_rubber", "Rubber Rounds", "Ammo", "-", 200, 1, 0.4, 1, 0, 0),
    EquipmentSpec("eq_ammo_tracer", "Tracer Rounds", "Ammo", "-", 450, 2, 1, 1.1, 0, 0),
    EquipmentSpec("eq_con_adrenaline", "Adrenaline", "Consumable", "-", 300, 2, 0, 1.25, 8, 2),
    EquipmentSpec("eq_con_antibiotic", "Antibiotics", "Consumable", "-", 260, 2, 25, 15, 2.2, 3),
    EquipmentSpec("eq_con_bandage", "Bandage", "Consumable", "-", 60, 1, 20, 0, 1.2, 8),
    EquipmentSpec("eq_con_energy", "Energy Drink", "Consumable", "-", 120, 1, 0, 1.15, 12, 4),
    EquipmentSpec("eq_con_food", "Street Food", "Consumable", "-", 80, 1, 15, 0, 1.8, 6),
    EquipmentSpec("eq_con_medkit", "Medkit", "Consumable", "-", 350, 1, 75, 25, 2.8, 3),
    EquipmentSpec("eq_con_painkiller", "Painkillers", "Consumable", "-", 150, 1, 30, 0, 0.8, 4),
    EquipmentSpec("eq_las_flashlight", "Weapon Light", "Attachment", "Laser", 350, 1, 0, 1, 0, 0),
    EquipmentSpec("eq_las_laser", "Laser Sight", "Attachment", "Laser", 500, 1, 0, 0.9, 0, 0),
    EquipmentSpec("eq_mag_drum", "Drum Magazine", "Attachment", "Magazine", 1500, 3, 35, 0, 0, 0),
    EquipmentSpec("eq_mag_extended", "Extended Magazine", "Attachment", "Magazine", 600, 1, 10, 0, 0, 0),
    EquipmentSpec("eq_mag_fast", "Fast Magazine", "Attachment", "Magazine", 900, 2, 0, 0, 0.75, 0),
    EquipmentSpec("eq_muz_brake", "Muzzle Brake", "Attachment", "Muzzle", 600, 1, 0.7, 1, 0, 0),
    EquipmentSpec("eq_muz_compensator", "Compensator", "Attachment", "Muzzle", 700, 1, 0.75, 1, 0, 0),
    EquipmentSpec("eq_muz_flash_hider", "Flash Hider", "Attachment", "Muzzle", 400, 1, 0.95, 1, 0, 0),
    EquipmentSpec("eq_muz_heavy_suppressor", "Heavy Suppressor", "Attachment", "Muzzle", 2600, 3, 0.4, 0.2, 0, 0),
    EquipmentSpec("eq_muz_suppressor", "Suppressor", "Attachment", "Muzzle", 1800, 2, 0.45, 0.35, 0, 0),
    EquipmentSpec("eq_opt_18x", "18x Scope", "Attachment", "Optic", 5200, 4, 18, 0, 0, 0),
    EquipmentSpec("eq_opt_3x", "3x Scope", "Attachment", "Optic", 900, 1, 3, 0, 0, 0),
    EquipmentSpec("eq_opt_4x", "4x Scope", "Attachment", "Optic", 1400, 2, 4, 0, 0, 0),
    EquipmentSpec("eq_opt_8x", "8x Scope", "Attachment", "Optic", 2600, 3, 8, 0, 0, 0),
    EquipmentSpec("eq_opt_red_dot", "Red Dot Sight", "Attachment", "Optic", 350, 1, 1.2, 0, 0, 0),
    EquipmentSpec("eq_opt_thermal", "Thermal 4x", "Attachment", "Optic", 6500, 4, 4, 0, 0, 0),
    EquipmentSpec("eq_stk_heavy", "Heavy Stock", "Attachment", "Stock", 1100, 2, 0, 0.65, 0, 0),
    EquipmentSpec("eq_stk_light", "Light Stock", "Attachment", "Stock", 450, 1, 0, 0.85, 0, 0),
    EquipmentSpec("eq_stk_tactical", "Tactical Stock", "Attachment", "Stock", 700, 1, 0, 0.75, 0, 0),
    EquipmentSpec("eq_sup_airstrike", "Airstrike", "Support", "-", 4200, 2, 0, 180, 18, 0),
    EquipmentSpec("eq_sup_ammo_drop", "Ammo Drop", "Support", "-", 400, 1, 0, 0, 30, 0),
    EquipmentSpec("eq_sup_anti_air", "Anti-Air Gun", "Support", "-", 5000, 3, 60, 150, 0, 0),
    EquipmentSpec("eq_sup_armoured_car", "Armoured Car", "Support", "-", 3500, 3, 75, 0, 0, 0),
    EquipmentSpec("eq_sup_gunship", "Gunship", "Support", "-", 7500, 4, 60, 90, 40, 0),
    EquipmentSpec("eq_sup_minefield", "Minefield", "Support", "-", 1200, 1, 180, 220, 16, 0),
    EquipmentSpec("eq_sup_mortar", "Mortar Team", "Support", "-", 3800, 3, 0, 100, 12, 0),
    EquipmentSpec("eq_sup_rocket_artillery", "Rocket Artillery", "Support", "-", 6500, 4, 0, 240, 22, 0),
    EquipmentSpec("eq_sup_tank", "Tank Support", "Support", "-", 9000, 5, 90, 200, 0, 0),
    EquipmentSpec("eq_sup_turret", "Auto Turret", "Support", "-", 2600, 2, 120, 45, 30, 0),
    EquipmentSpec("eq_sup_uav", "UAV Scan", "Support", "-", 800, 1, 25, 120, 0, 0),
    EquipmentSpec("eq_sup_uav_jammer", "Radio Jammer Drone", "Support", "-", 1600, 2, 0, 0, 20, 0),
    EquipmentSpec("eq_sup_uav_strike", "Strike Drone", "Support", "-", 2200, 2, 0, 0, 0, 0),
    EquipmentSpec("eq_und_angled_grip", "Angled Grip", "Attachment", "Underbarrel", 450, 1, 0, 0.85, 0, 0),
    EquipmentSpec("eq_und_bipod", "Bipod", "Attachment", "Underbarrel", 800, 2, 0, 0.7, 0, 0),
    EquipmentSpec("eq_und_grenade_launcher", "Underbarrel Launcher", "Attachment", "Underbarrel", 3200, 4, 0, 1, 1, 0),
    EquipmentSpec("eq_und_vertical_grip", "Vertical Grip", "Attachment", "Underbarrel", 500, 1, 0, 0.8, 0, 0),
    EquipmentSpec("eq_upg_armour", "Body Armour", "Upgrade", "-", 1200, 2, 5, 0, 0, 0),
    EquipmentSpec("eq_upg_brakes", "Brake Upgrade", "Upgrade", "-", 1200, 1, 5, 0, 0, 0),
    EquipmentSpec("eq_upg_engine", "Engine Tuning", "Upgrade", "-", 1200, 1, 5, 0, 0, 0),
    EquipmentSpec("eq_upg_nitro", "Nitro Kit", "Upgrade", "-", 1200, 2, 5, 0, 0, 0),
    EquipmentSpec("eq_upg_tires", "Tire Upgrade", "Upgrade", "-", 1200, 1, 5, 0, 0, 0),
    EquipmentSpec("eq_upg_weight", "Weight Reduction", "Upgrade", "-", 1200, 3, 5, 0, 0, 0),
)


@dataclasses.dataclass(frozen=True)
class ShopSpec:
    item_id: str
    display_name: str
    item_type: str
    price: int
    unlock_level: int


SHOP_ITEMS: tuple[ShopSpec, ...] = (
    ShopSpec("emote_dance", "Dance Emote", "Emote", 600, 1),
    ShopSpec("emote_laugh", "Laugh Emote", "Emote", 400, 1),
    ShopSpec("skin_gold", "Gold Suit", "Skin", 3000, 2),
    ShopSpec("skin_shadow", "Shadow Outfit", "Skin", 3500, 2),
    ShopSpec("skin_military", "Military Dress", "Skin", 30000, 19),
    ShopSpec("vehicle_apc", "Armoured Carrier", "Vehicle", 55000, 18),
    ShopSpec("vehicle_bike", "Street Bike", "Vehicle", 5000, 2),
    ShopSpec("vehicle_sport_bike", "Sport Bike", "Vehicle", 6500, 3),
    ShopSpec("vehicle_bus", "City Bus", "Vehicle", 9000, 3),
    ShopSpec("vehicle_helicopter", "Helicopter", "Vehicle", 45000, 19),
    ShopSpec("vehicle_monster_truck", "Monster Truck", "Vehicle", 12000, 4),
    ShopSpec("vehicle_muscle", "Muscle GT", "Vehicle", 6000, 2),
    ShopSpec("vehicle_taxi", "Taxi", "Vehicle", 4000, 2),
    ShopSpec("weapon_ak_rifle", "AK Rifle", "Weapon", 2400, 2),
    ShopSpec("weapon_crowbar", "Crowbar", "Weapon", 900, 1),
    ShopSpec("weapon_katana", "Katana", "Weapon", 3000, 3),
    ShopSpec("weapon_knife", "Knife", "Weapon", 700, 1),
    ShopSpec("weapon_lmg", "LMG", "Weapon", 9000, 4),
    ShopSpec("weapon_railgun", "Railgun", "Weapon", 40000, 17),
    ShopSpec("weapon_revolver", "Revolver .44", "Weapon", 1500, 2),
    ShopSpec("weapon_vector_smg", "Vector SMG", "Weapon", 1200, 1),
    ShopSpec("weapon_shotgun", "Pump Shotgun", "Weapon", 1800, 1),
    ShopSpec("weapon_sledgehammer", "Sledgehammer", "Weapon", 2600, 3),
    ShopSpec("weapon_sniper", "Bolt Sniper", "Weapon", 4500, 2),
    ShopSpec("weapon_uzi", "Uzi", "Weapon", 2000, 2),
)


@dataclasses.dataclass(frozen=True)
class MissionSpec:
    mission_id: str
    title: str
    rewards_money: int
    rewards_reputation: int
    time_limit_seconds: float
    fail_at_wanted_level: int
    start_trigger: str
    objectives: tuple[str, ...]
    completion_conditions: tuple[str, ...]


MISSIONS: tuple[MissionSpec, ...] = (
    MissionSpec(
        "mis_01_wake_up",
        "Wake Up, Dude",
        750,
        10,
        360,
        3,
        "trigger_start_apartment",
        ("Get out of bed", "Walk to the street", "Steal a car"),
        ("car_stolen",),
    ),
    MissionSpec(
        "mis_02_delivery",
        "Special Delivery",
        900,
        20,
        300,
        3,
        "trigger_delivery_call",
        ("Pick up the package", "Deliver the package to the pier", "Lose the cops"),
        ("package_delivered", "not_wanted"),
    ),
    MissionSpec(
        "mis_03_rooftop",
        "Rooftop Run",
        1200,
        30,
        240,
        3,
        "trigger_rooftop_call",
        ("Reach the rooftop", "Escape before the timer ends"),
        ("rooftop_cleared",),
    ),
    MissionSpec(
        "mis_04_car_meet",
        "Car Meet",
        1500,
        40,
        240,
        3,
        "trigger_car_meet",
        ("Win the race", "Bring the car back in one piece"),
        ("race_won", "vehicle_alive"),
    ),
    MissionSpec(
        "mis_05_showdown",
        "Showdown",
        2500,
        60,
        300,
        3,
        "trigger_showdown",
        ("Survive the ambush", "Take out the last wave"),
        ("ambush_survived", "final_wave_cleared"),
    ),
    MissionSpec(
        "mis_06_chop_shop",
        "Chop Shop",
        3800,
        55,
        300,
        3,
        "trigger_chop_shop",
        ("Steal the marked car and deliver it to the chop shop", "Lose the cops"),
        ("car_delivered", "not_wanted"),
    ),
    MissionSpec(
        "mis_07_heist",
        "The Heist",
        6000,
        70,
        300,
        4,
        "trigger_heist",
        ("Get inside", "Grab the cash", "Escape the alarm"),
        ("cash_taken", "alarm_escaped"),
    ),
    MissionSpec(
        "mis_08_getaway",
        "Getaway",
        6500,
        90,
        360,
        4,
        "trigger_getaway",
        ("Reach the parking lot", "Survive the roadblock", "Lose the helicopter"),
        ("parking_reached", "roadblock_survived", "heli_lost"),
    ),
    MissionSpec(
        "mis_09_night_run",
        "Night Run",
        7500,
        110,
        420,
        4,
        "trigger_night_run",
        ("Wait for nightfall", "Load the cargo at the port", "Deliver it before sunrise"),
        ("night_fallen", "cargo_loaded", "cargo_delivered"),
    ),
    MissionSpec(
        "mis_10_slum_deal",
        "Slum Deal",
        8500,
        120,
        360,
        4,
        "trigger_slum_deal",
        ("Find the contact in the slums", "Survive the ambush", "Get the package out"),
        ("contact_met", "ambush_survived", "package_secured"),
    ),
    MissionSpec(
        "mis_11_airport_heist",
        "Airport Heist",
        9500,
        130,
        360,
        4,
        "trigger_airport_heist",
        ("Get past the terminal gate", "Take the cargo from the hangar", "Escape the runway"),
        ("terminal_passed", "hangar_looted", "runway_cleared"),
    ),
    MissionSpec(
        "mis_12_military_run",
        "Military Run",
        10500,
        140,
        420,
        4,
        "trigger_military_run",
        ("Sneak past the checkpoint", "Steal the truck", "Lose the patrol"),
        ("checkpoint_passed", "truck_stolen", "patrol_lost"),
    ),
    MissionSpec(
        "mis_13_room_302",
        "Room 302",
        11500,
        150,
        360,
        4,
        "trigger_room_302",
        ("Book a room at the hotel", "Search room 302", "Leave before the call ends"),
        ("room_booked", "room_searched", "call_ended"),
    ),
    MissionSpec(
        "mis_14_rooftop_chase",
        "Rooftop Chase",
        13000,
        160,
        300,
        4,
        "trigger_rooftop_chase",
        ("Follow the target to the roof", "Catch him before he jumps", "Get down alive"),
        ("roof_reached", "target_caught", "descent_survived"),
    ),
    MissionSpec(
        "mis_15_showroom_hit",
        "Showroom Hit",
        14500,
        170,
        360,
        4,
        "trigger_showroom_hit",
        ("Walk into the showroom", "Take the keys", "Drive through the window"),
        ("showroom_entered", "keys_taken", "window_crashed"),
    ),
    MissionSpec(
        "mis_16_final_chaos",
        "Final Chaos",
        16000,
        190,
        480,
        4,
        "trigger_final_chaos",
        ("Reach the downtown tower", "Hold the lobby", "Survive the last wave"),
        ("tower_reached", "lobby_held", "wave_survived"),
    ),
    MissionSpec(
        "mis_17_prison_break",
        "Prison Break",
        18000,
        60,
        420,
        4,
        "trigger_prison_break",
        ("Tail the transport van", "Free the contact", "Lose the pursuit"),
        ("van_tailed", "contact_freed", "pursuit_lost"),
    ),
    MissionSpec(
        "mis_18_army_depot",
        "Army Depot",
        20000,
        65,
        420,
        4,
        "trigger_army_depot",
        ("Cut the fence", "Load the crates", "Drive out before the alarm"),
        ("fence_cut", "crates_loaded", "alarm_beaten"),
    ),
    MissionSpec(
        "mis_19_hotel_siege",
        "Hotel Siege",
        22500,
        70,
        360,
        4,
        "trigger_hotel_siege",
        ("Rent the top floor", "Hold the stairwell", "Escape over the roof"),
        ("floor_rented", "stairwell_held", "roof_escaped"),
    ),
    MissionSpec(
        "mis_20_endgame",
        "Endgame",
        26000,
        75,
        540,
        4,
        "trigger_endgame",
        ("Call in support", "Take the airfield", "Walk away clean"),
        ("support_called", "airfield_taken", "clean_exit"),
    ),
)


@dataclasses.dataclass(frozen=True)
class DistrictSpec:
    district_id: str
    display_name_key: str
    kind: str
    centre_x: float
    centre_z: float
    size_x: float
    size_z: float
    requires_pass: bool
    camera_height_hint: float
    density_multiplier: float


DISTRICTS: tuple[DistrictSpec, ...] = (
    DistrictSpec("district_airport", "ui.district.airport", "Airport", 1100, 700, 900, 500, True, 6.0, 1.0),
    DistrictSpec("district_downtown", "ui.district.downtown", "Downtown", 0, 0, 600, 600, False, 5.0, 1.0),
    DistrictSpec("district_farm", "ui.district.farm", "Farm", -1200, 900, 600, 600, False, 4.5, 1.0),
    DistrictSpec("district_hotel", "ui.district.hotel", "Hotel", 150, 150, 150, 150, False, 4.5, 1.0),
    DistrictSpec("district_market", "ui.district.market", "Market", 400, -500, 300, 300, False, 4.2, 1.1),
    DistrictSpec("district_military", "ui.district.military", "Military", 600, 1200, 500, 400, True, 5.5, 1.0),
    DistrictSpec("district_port", "ui.district.port", "Port", 900, -900, 700, 400, False, 5.0, 0.9),
    DistrictSpec("district_residential", "ui.district.residential", "Residential", -700, 300, 500, 500, False, 4.5, 1.0),
    DistrictSpec("district_restaurant", "ui.district.restaurant", "Restaurant", 350, 250, 200, 200, False, 4.2, 1.0),
    DistrictSpec("district_secret", "ui.district.secret", "Secret", 100, -100, 120, 120, True, 4.0, 1.0),
    DistrictSpec("district_showroom", "ui.district.showroom", "Showroom", -250, -350, 250, 250, False, 4.5, 1.0),
    DistrictSpec("district_slums", "ui.district.slums", "Slums", -800, -600, 400, 400, False, 4.3, 1.0),
)


def generate_weapons() -> int:
    for w in WEAPONS:
        slug = slugify_title(w.display_name)
        asset_name = f"WPN_{slug}"
        weapon_id = f"wpn_{slug.lower()}"
        rel_path = f"Assets/Data/Weapons/{asset_name}.asset"
        body = (
            monobehaviour_header(asset_name, "WeaponData")
            + f"  weaponId: {yaml_str(weapon_id)}\n"
            + f"  displayName: {yaml_str(w.display_name)}\n"
            + f"  weaponClass: {WEAPON_CLASSES[w.weapon_class]}\n"
            + f"  damage: {fmt_num(w.damage)}\n"
            + f"  pellets: {w.pellets}\n"
            + "  headMultiplier: 3.2\n"
            + "  limbMultiplier: 0.75\n"
            + f"  fireRate: {fmt_num(w.fire_rate)}\n"
            + f"  spread: {fmt_num(w.spread)}\n"
            + "  recoilCurve:\n"
            + animation_curve_yaml("    ", 0.2, 1.0)
            + f"  range: {fmt_num(w.range_m)}\n"
            + f"  magazineSize: {w.magazine_size}\n"
            + f"  reloadTime: {fmt_num(w.reload_time)}\n"
            + f"  ragdollMultiplier: {fmt_num(w.ragdoll_multiplier)}\n"
            + f"  isAutomatic: {1 if w.is_automatic else 0}\n"
            + f"  mechanicName: {yaml_str(w.mechanic)}\n"
            + f"  burstCount: {w.burst_count}\n"
            + f"  chargeSeconds: {fmt_num(w.charge_seconds)}\n"
            + f"  projectileSpeed: {fmt_num(w.projectile_speed)}\n"
            + f"  selfDamage: {fmt_num(w.self_damage)}\n"
            + f"  heatPerShot: {fmt_num(w.heat_per_shot)}\n"
            + f"  muzzleEnergyJoules: {fmt_num(w.muzzle_energy_joules)}\n"
            + "  fireSound: {fileID: 0}\n"
            + "  reloadSound: {fileID: 0}\n"
            + "  muzzleFlashPrefab: {fileID: 0}\n"
        )
        write_asset_and_meta(rel_path, body)
    return len(WEAPONS)


def generate_vehicles() -> int:
    for v in VEHICLES:
        slug = slugify_title(v.vehicle_name)
        asset_name = f"VEH_{slug}"
        vehicle_id = f"veh_{slug.lower()}"
        rel_path = f"Assets/Data/Vehicles/{asset_name}.asset"
        body = (
            monobehaviour_header(asset_name, "VehicleData")
            + f"  vehicleId: {yaml_str(vehicle_id)}\n"
            + f"  vehicleName: {yaml_str(v.vehicle_name)}\n"
            + f"  handlingProfile: {HANDLING_PROFILES[v.handling_profile]}\n"
            + f"  armourClass: {ARMOUR_CLASSES[v.armour_class]}\n"
            + f"  maxHealth: {fmt_num(v.max_health)}\n"
            + f"  seats: {v.seats}\n"
            + f"  centerOfMassOffset: {{x: 0, y: {fmt_num(v.com_y)}, z: 0}}\n"
            + f"  maxSpeed: {fmt_num(v.max_speed)}\n"
            + f"  acceleration: {fmt_num(v.acceleration)}\n"
            + f"  brakeForce: {fmt_num(v.brake_force)}\n"
            + f"  steerAngle: {fmt_num(v.steer_angle)}\n"
            + "  enterClip: {fileID: 0}\n"
            + "  exitClip: {fileID: 0}\n"
            + "  engineSound: {fileID: 0}\n"
            + "  hornSound: {fileID: 0}\n"
        )
        write_asset_and_meta(rel_path, body)
    return len(VEHICLES)


def generate_equipment() -> int:
    for eq in EQUIPMENT:
        suffix = eq.equipment_id[3:] if eq.equipment_id.startswith("eq_") else eq.equipment_id
        asset_name = f"EQ_{suffix}"
        rel_path = f"Assets/Data/Equipment/{asset_name}.asset"
        body = (
            monobehaviour_header(asset_name, "EquipmentData")
            + f"  equipmentId: {yaml_str(eq.equipment_id)}\n"
            + f"  displayName: {yaml_str(eq.display_name)}\n"
            + f"  kind: {EQUIPMENT_KINDS[eq.kind]}\n"
            + f"  slot: {ATTACHMENT_SLOTS[eq.slot]}\n"
            + f"  price: {eq.price}\n"
            + f"  unlockLevel: {eq.unlock_level}\n"
            + f"  valueA: {fmt_num(eq.value_a)}\n"
            + f"  valueB: {fmt_num(eq.value_b)}\n"
            + f"  valueC: {fmt_num(eq.value_c)}\n"
            + f"  valueD: {fmt_num(eq.value_d)}\n"
            + "  allowedClasses: \n"
        )
        write_asset_and_meta(rel_path, body)
    return len(EQUIPMENT)


def generate_shop() -> int:
    for item in SHOP_ITEMS:
        asset_name = f"SHP_{item.item_id}"
        rel_path = f"Assets/Data/Shop/{asset_name}.asset"
        body = (
            monobehaviour_header(asset_name, "ShopItem")
            + f"  itemId: {yaml_str(item.item_id)}\n"
            + f"  displayName: {yaml_str(item.display_name)}\n"
            + f"  itemType: {SHOP_ITEM_TYPES[item.item_type]}\n"
            + f"  price: {item.price}\n"
            + f"  unlockLevel: {item.unlock_level}\n"
            + "  prefabReference: {fileID: 0}\n"
        )
        write_asset_and_meta(rel_path, body)
    return len(SHOP_ITEMS)


def generate_missions() -> int:
    for m in MISSIONS:
        suffix = m.mission_id[4:] if m.mission_id.startswith("mis_") else m.mission_id
        asset_name = f"MIS_{suffix}"
        rel_path = f"Assets/Data/Missions/{asset_name}.asset"
        dialogue = MISSION_DIALOGUE[m.mission_id]
        time_bonus = round(0.20 * m.rewards_money / m.time_limit_seconds, 4)
        obj_lines = "".join(f"  - {yaml_str(o)}\n" for o in m.objectives)
        cond_lines = "".join(f"  - {yaml_str(c)}\n" for c in m.completion_conditions)
        dlg_lines = "".join(f"  - {yaml_str(d)}\n" for d in dialogue)
        body = (
            monobehaviour_header(asset_name, "MissionData")
            + f"  missionId: {yaml_str(m.mission_id)}\n"
            + f"  title: {yaml_str(m.title)}\n"
            + "  objectives:\n"
            + obj_lines
            + f"  rewardsMoney: {m.rewards_money}\n"
            + f"  rewardsReputation: {m.rewards_reputation}\n"
            + f"  startTrigger: {yaml_str(m.start_trigger)}\n"
            + "  completionConditions:\n"
            + cond_lines
            + "  dialogueLines:\n"
            + dlg_lines
            + f"  timeLimitSeconds: {fmt_num(m.time_limit_seconds)}\n"
            + f"  failAtWantedLevel: {m.fail_at_wanted_level}\n"
            + "  retryable: 1\n"
            + f"  timeBonusPerSecond: {fmt_num(time_bonus)}\n"
        )
        write_asset_and_meta(rel_path, body)
    return len(MISSIONS)


def generate_districts() -> int:
    for d in DISTRICTS:
        asset_name = d.district_id
        rel_path = f"Assets/Data/Districts/{asset_name}.asset"
        body = (
            monobehaviour_header(asset_name, "DistrictDefinition")
            + f"  districtId: {yaml_str(d.district_id)}\n"
            + f"  displayNameKey: {yaml_str(d.display_name_key)}\n"
            + f"  kind: {DISTRICT_KINDS[d.kind]}\n"
            + f"  centre: {{x: {fmt_num(d.centre_x)}, y: {fmt_num(d.centre_z)}}}\n"
            + f"  size: {{x: {fmt_num(d.size_x)}, y: {fmt_num(d.size_z)}}}\n"
            + f"  requiresPass: {1 if d.requires_pass else 0}\n"
            + f"  cameraHeightHint: {fmt_num(d.camera_height_hint)}\n"
            + f"  densityMultiplier: {fmt_num(d.density_multiplier)}\n"
        )
        write_asset_and_meta(rel_path, body)
    return len(DISTRICTS)


def generate_settings_and_misc() -> int:
    # 1. MovementSettings
    write_asset_and_meta(
        "Assets/Settings/MovementSettings.asset",
        monobehaviour_header("MovementSettings", "MovementSettings")
        + "  deadZone: 0.18\n"
        + "  walkSpeed: 3.2\n"
        + "  runSpeed: 6\n"
        + "  sprintSpeed: 8.8\n"
        + "  crouchSpeed: 2.1\n"
        + "  sprintFovKick: 5\n"
        + "  coyoteTime: 0.12\n"
        + "  jumpBufferTime: 0.1\n"
        + "  gravity: -22\n"
        + "  jumpHeight: 1.35\n"
        + "  landSoftThreshold: -8\n"
        + "  landHardThreshold: -14\n"
        + "  standingHeight: 1.8\n"
        + "  crouchHeight: 1.1\n",
    )

    # 2. RagdollSettings
    write_asset_and_meta(
        "Assets/Settings/RagdollSettings.asset",
        monobehaviour_header("RagdollSettings", "RagdollSettings")
        + "  fallRagdollThreshold: -16\n"
        + "  highForceImpulseThreshold: 55\n"
        + "  recoveryVelocity: 1.1\n"
        + "  recoveryHoldTime: 1.8\n"
        + "  getUpDuration: 0.9\n"
        + "  totalMass: 78\n"
        + "  maxFullSimRagdolls: 12\n"
        + "  fullSimRadius: 55\n"
        + "  sleepVelocity: 0.5\n"
        + "  pelvisFollowLag: 0.08\n",
    )

    # 3. CombatSettings
    write_asset_and_meta(
        "Assets/Settings/CombatSettings.asset",
        monobehaviour_header("CombatSettings", "CombatSettings")
        + "  lightMeleeForce: 1200\n"
        + "  lightHitStop: 0.04\n"
        + "  heavyMeleeForce: 3800\n"
        + "  heavyHitStop: 0.11\n"
        + "  ragdolledDamageMultiplier: 1.6\n"
        + "  backstabMultiplier: 1.8\n"
        + "  backstabDotThreshold: -0.35\n"
        + "  muzzleLightDuration: 0.055\n"
        + "  recoilRecoveryTime: 0.18\n"
        + "  pointBlankDistance: 2.5\n"
        + "  baseShotImpulse: 45\n"
        + "  shotUpwardBias: 0.25\n"
        + "  explosionRadius: 6\n"
        + "  explosionImpulse: 260\n"
        + "  explosionDamage: 140\n"
        + "  explosionBufferSize: 64\n",
    )

    # 4. VehicleSettings
    write_asset_and_meta(
        "Assets/Settings/VehicleSettings.asset",
        monobehaviour_header("VehicleSettings", "VehicleSettings")
        + "  enterDuration: 0.45\n"
        + "  exitDuration: 0.45\n"
        + "  ejectSpeedThreshold: 9\n"
        + "  ejectForwardImpulse: 900\n"
        + "  ejectUpImpulse: 420\n"
        + "  exitOffset: 1.4\n"
        + "  smokeHealthRatio: 0.5\n"
        + "  criticalHealthRatio: 0.25\n"
        + "  destructionDelay: 2.8\n"
        + "  steeringFalloff: 0.35\n"
        + "  downforce: 12\n",
    )

    # 5. WantedSettings
    thresholds_hex = encode_int_array_hex([1, 4, 9, 16, 25, 36])
    units_hex = encode_int_array_hex([2, 3, 4, 5, 6, 6])
    write_asset_and_meta(
        "Assets/Settings/WantedSettings.asset",
        monobehaviour_header("WantedSettings", "WantedSettings")
        + f"  thresholds: {thresholds_hex}\n"
        + "  cooldowns:\n"
        + "  - 6\n"
        + "  - 9\n"
        + "  - 12\n"
        + "  - 16\n"
        + "  - 20\n"
        + "  - 25\n"
        + "  searchRadius: 48\n"
        + "  memorySeconds: 9\n"
        + "  assaultCivilianWeight: 1\n"
        + "  assaultPoliceWeight: 2\n"
        + "  killPoliceWeight: 3\n"
        + "  weaponDischargeWeight: 1\n"
        + "  vehicleTheftWeight: 2\n"
        + "  runOverWeight: 2\n"
        + "  explosionWeight: 2\n"
        + "  destroyPoliceVehicleWeight: 3\n"
        + f"  unitsPerLevel: {units_hex}\n"
        + "  helicopterFromLevel: 3\n"
        + "  roadblockFromLevel: 3\n"
        + "  swatFromLevel: 4\n"
        + "  openFireFromLevel: 2\n",
    )

    # 6. QualityPreset_Medium
    write_asset_and_meta(
        "Assets/Settings/QualityPreset_Medium.asset",
        monobehaviour_header("QualityPreset_Medium", "QualitySettingsPreset")
        + "  presetName: \"Medium\"\n"
        + "  shadowDistance: 45\n"
        + "  particleBudget: 256\n"
        + "  drawDistance: 180\n"
        + "  streamerUnloadInterval: 2\n",
    )

    # 7. DefaultCameraShake
    write_asset_and_meta(
        "Assets/Settings/DefaultCameraShake.asset",
        monobehaviour_header("DefaultCameraShake", "CameraShakeProfile")
        + "  profileId: \"default\"\n"
        + "  amplitude: 1\n"
        + "  frequency: 18\n"
        + "  duration: 0.18\n"
        + "  falloff:\n"
        + animation_curve_yaml("    ", 1.0, 0.0),
    )

    # 8. IMP_Concrete
    write_asset_and_meta(
        "Assets/Data/Impacts/IMP_Concrete.asset",
        monobehaviour_header("IMP_Concrete", "ImpactMaterialData")
        + "  materialType: 1\n"
        + "  impactPrefab: {fileID: 0}\n"
        + "  decalPrefab: {fileID: 0}\n"
        + "  sound: {fileID: 0}\n",
    )

    # 9 & 10. Emotes
    write_asset_and_meta(
        "Assets/Data/Emotes/EMO_Dance.asset",
        monobehaviour_header("EMO_Dance", "EmoteData")
        + "  id: \"emote_dance\"\n"
        + "  displayName: \"Dance\"\n"
        + "  clip: {fileID: 0}\n"
        + "  duration: 2.5\n",
    )
    write_asset_and_meta(
        "Assets/Data/Emotes/EMO_Laugh.asset",
        monobehaviour_header("EMO_Laugh", "EmoteData")
        + "  id: \"emote_laugh\"\n"
        + "  displayName: \"Laugh\"\n"
        + "  clip: {fileID: 0}\n"
        + "  duration: 2\n",
    )
    return 10


def generate_urp_assets() -> int:
    renderer_guid = unity_guid("Assets/Settings/Mobile_Renderer.asset")
    default_vol_guid = unity_guid("Assets/DefaultVolumeProfile.asset")

    # 1. Assets/Settings/Mobile_Renderer.asset (§1.9.1: Forward m_RenderingMode: 0, no features)
    write_asset_and_meta(
        "Assets/Settings/Mobile_Renderer.asset",
        monobehaviour_header("Mobile_Renderer", "UniversalRendererData")
        + "  m_AssetVersion: 2\n"
        + "  m_RendererFeatures: []\n"
        + "  m_RendererFeatureMap: []\n"
        + "  m_OpaqueLayerMask:\n"
        + "    serializedVersion: 2\n"
        + "    m_Bits: 4294967295\n"
        + "  m_TransparentLayerMask:\n"
        + "    serializedVersion: 2\n"
        + "    m_Bits: 4294967295\n"
        + "  m_DefaultStencilState:\n"
        + "    m_OverrideStencilState: 0\n"
        + "    m_StencilReference: 0\n"
        + "    m_StencilCompareFunction: 8\n"
        + "    m_PassOperation: 0\n"
        + "    m_FailOperation: 0\n"
        + "    m_ZFailOperation: 0\n"
        + "  m_ShadowTransparentReceive: 0\n"
        + "  m_RenderingMode: 0\n"
        + "  m_DepthPrimingMode: 0\n"
        + "  m_CopyDepthMode: 0\n"
        + "  m_IntermediateTextureMode: 0\n",
    )

    # 2. Assets/Settings/Mobile_URP.asset (§1.9.1)
    write_asset_and_meta(
        "Assets/Settings/Mobile_URP.asset",
        monobehaviour_header("Mobile_URP", "UniversalRenderPipelineAsset")
        + "  k_AssetVersion: 12\n"
        + "  m_RendererDataList:\n"
        + f"  - {{fileID: 11400000, guid: {renderer_guid}, type: 2}}\n"
        + "  m_DefaultRendererIndex: 0\n"
        + "  m_SupportsCameraDepthTexture: 0\n"
        + "  m_SupportsCameraOpaqueTexture: 0\n"
        + "  m_SupportsHDR: 1\n"
        + "  m_HDRColorBufferPrecision: 0\n"
        + "  m_MSAA: 1\n"
        + "  m_RenderScale: 1\n"
        + "  m_MainLightRenderingMode: 1\n"
        + "  m_MainLightShadowsSupported: 1\n"
        + "  m_MainLightShadowmapResolution: 2048\n"
        + "  m_AdditionalLightsRenderingMode: 1\n"
        + "  m_AdditionalLightsPerObjectLimit: 4\n"
        + "  m_AdditionalLightShadowsSupported: 0\n"
        + "  m_AdditionalLightsShadowmapResolution: 512\n"
        + "  m_ShadowDistance: 50\n"
        + "  m_ShadowCascadeCount: 1\n"
        + "  m_Cascade2Split: 0.25\n"
        + "  m_Cascade3Split: {x: 0.1, y: 0.3}\n"
        + "  m_Cascade4Split: {x: 0.067, y: 0.2, z: 0.467}\n"
        + "  m_ShadowDepthBias: 1\n"
        + "  m_ShadowNormalBias: 1\n"
        + "  m_SoftShadowsSupported: 0\n"
        + "  m_UseSRPBatcher: 1\n"
        + "  m_SupportsDynamicBatching: 0\n"
        + "  m_MixedLightingSupported: 0\n"
        + "  m_VolumeFrameworkUpdateMode: 0\n"
        + "  m_VolumeProfile: {fileID: 0}\n",
    )

    # 3. Assets/DefaultVolumeProfile.asset (§1.9.6: zero volume components)
    write_asset_and_meta(
        "Assets/DefaultVolumeProfile.asset",
        monobehaviour_header("DefaultVolumeProfile", "VolumeProfile")
        + "  m_Components: []\n",
    )

    # 4. Assets/UniversalRenderPipelineGlobalSettings.asset (§1.9.6)
    write_asset_and_meta(
        "Assets/UniversalRenderPipelineGlobalSettings.asset",
        monobehaviour_header("UniversalRenderPipelineGlobalSettings", "UniversalRenderPipelineGlobalSettings")
        + "  k_AssetVersion: 8\n"
        + f"  m_DefaultVolumeProfile: {{fileID: 11400000, guid: {default_vol_guid}, type: 2}}\n"
        + "  m_RenderingLayerNames:\n"
        + "  - Default\n"
        + "  m_StripDebugVariants: 1\n"
        + "  m_StripUnusedPostProcessingVariants: 1\n"
        + "  m_StripUnusedVariants: 1\n",
    )
    return 4


def generate_folder_metas() -> None:
    folders = (
        "Assets/Data",
        "Assets/Data/Weapons",
        "Assets/Data/Vehicles",
        "Assets/Data/Equipment",
        "Assets/Data/Shop",
        "Assets/Data/Missions",
        "Assets/Data/Districts",
        "Assets/Data/Impacts",
        "Assets/Data/Emotes",
        "Assets/Settings",
    )
    for folder in folders:
        write_folder_meta(folder)


def main() -> int:
    errs = validate_story_lines()
    if errs:
        for err in errs:
            print(f"STORY ERROR: {err}", file=sys.stderr)
        return 1

    generate_folder_metas()
    counts = {
        "weapons": generate_weapons(),
        "vehicles": generate_vehicles(),
        "equipment": generate_equipment(),
        "shop": generate_shop(),
        "missions": generate_missions(),
        "districts": generate_districts(),
        "settings_misc": generate_settings_and_misc(),
        "urp": generate_urp_assets(),
    }
    total = sum(counts.values())
    print(
        f"Generated {total} assets: "
        + ", ".join(f"{k}={v}" for k, v in counts.items())
    )
    if total != 196:
        print(f"ERROR: expected 196 assets, generated {total}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
