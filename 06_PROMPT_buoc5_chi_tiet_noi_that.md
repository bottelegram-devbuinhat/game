# BƯỚC 5 — Chi tiết nhỏ (decal) và nội thất khí tài / buồng điều khiển

Bước 5/5 — 42 mục: 13 chi tiết nhỏ dạng decal (593–683) + 29 không gian chui vào được (700–728). Bước nặng nhất về số mục, cắt phiên trong cùng file này.

Đọc `00_PROMPT_TONG.md` trước file này nếu chưa đọc: nó có quy ước nền trắng, khung ảnh, cách đặt
tên, gate kiểm và luật không bịa kết quả. File này chỉ chứa nội dung prompt — copy nguyên khối, không
rút gọn, không gộp hai mục vào một ảnh.

**Nguồn:** `11_chi_tiet_nho.md` (file cũ, đã gộp vào bước này), `12_noi_that_khi_tai_dieu_khien.md` (file cũ, đã gộp vào bước này). Gộp nguyên văn, số hiệu mục giữ đúng như bản gốc.

Đếm của bước này (phải khớp trước khi đóng zip):

```bash
grep -h '^### ' prompts/06_buoc5_chi_tiet_noi_that.md | wc -l   # phải ra 42
```

---

## Chi tiết nhỏ — 13 mục còn lại

*Nguồn: `11_chi_tiet_nho.md` (file cũ, đã gộp vào bước này) — 13 mục, copy nguyên khối bên dưới.*

*Ghi chú: file này chỉ còn mục chưa đạt. 565 prompt của ảnh đã duyệt đã xoá khỏi danh sách đang làm (bản đầy đủ 683 mục vẫn nằm trong git, commit `d332e04`) — đừng sinh lại các mục đó.*

Khuôn: 1024x1024, decal nhìn thẳng từ trên. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 593. Máy lạnh tường 1
*0,90 × 0,30 × 0,60 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
3D game prop, wall-mounted split AC indoor unit, white plastic shell, horizontal louvres, fine grille, small brand-free display, 0,90 × 0,30 × 0,60 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 598. Hộp điện
*0,40 × 0,20 × 0,60 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
3D game prop, grey utility cabinet, hinged door with latch, warning sticker without text, cable glands along the bottom, 0,40 × 0,20 × 0,60 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 611. Bồn nước mái
*Ø 1,5 m, cao 2 m*
*Lỗi lô trước: có vòng ống rời rạc lơ lửng dưới chân bồn nước*

```
3D game prop, rooftop water tank, blue polyethylene cylinder with lid and inlet, steel support legs with cross braces, no loose pipes, 1,5 m diameter, 2 m tall, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 623. Vệt phanh
*dài 6 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, twin tyre skid marks, dark rubber streaks with worn edges, slight curve, 6 m long, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 626. Miếng vá đường
*1 × 1 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh; là khối 3D dày 2–3 cm thay vì miếng vá phẳng sát mặt đường*

```
flat decal, asphalt repair patch, darker rectangular patch with a ragged tar edge, zero thickness, flush with the road, 1 × 1 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 627. Vết ố tường
*1 × 2 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, wall grime and water stains, vertical streaks, mildew spots, soft irregular edges, 1 × 2 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 640. Poster rách
*0,50 × 0,70 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, torn paper poster, peeling corners, faded blank print, damp stains, 0,50 × 0,70 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 650. Phụ tùng xe
*0,60 × 0,40 m*
*Lỗi lô trước: dính gizmo trục màu (đỏ/xanh) của viewport*

```
3D game prop, discarded car bumper with cracked paint and a detached hubcap beside it, 0,60 × 0,40 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 651. Xe đạp cũ
*dài 1,7 m*
*Lỗi lô trước: có mảng tường/góc phòng phía sau, không phải nền trơn*

```
3D game prop, old bicycle, rusted frame, bent fender, worn saddle, flat tyres, chain hanging loose, 1,7 m long, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 670. Vết đạn tường
*0,5 × 0,5 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, bullet hole cluster, six holes with chipped plaster rings and hairline cracks, 0,5 × 0,5 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 671. Vết cháy
*1 × 1 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, scorch mark, soot-black centre fading to brown with cracked edges, 1 × 1 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 673. Lá khô rải
*2 × 2 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
flat decal, scattered dry leaves, brown and yellow curled leaves, 2 × 2 m, flat decal, top-down orthographic view, zero thickness, edges fading into the background, flat pure white #FFFFFF background, single tone, no cast shadow, no gradient, no checkerboard, no dot grid, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 683. Ghế chờ
*1,6 × 0,5 × 0,8 m*
*Lỗi lô trước: dính gizmo trục màu dưới ghế*

```
3D game prop, row of three linked waiting chairs, blue moulded plastic seats, chrome tubular frame, shared armrests, 1,6 × 0,5 × 0,8 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

---

## Nội thất khí tài, buồng điều khiển và không gian chơi vào được — 29 mục mới

*Nguồn: `12_noi_that_khi_tai_dieu_khien.md` (file cũ, đã gộp vào bước này) — 29 mục, copy nguyên khối bên dưới.*

*Ghi chú: mục 700–728. Đây là các không gian nhân vật **nhìn thấy hoặc chui vào được** nhưng bộ 683 mục
cũ không có: buồng lái máy bay/trực thăng, khoang xe tăng/APC, góc nhìn và HUD của UAV, bàn điều khiển
tên lửa, trạm điều khiển UAV, cùng nội thất các công trình trước đây chỉ có vỏ ngoài (nhà ở, cửa hàng,
văn phòng, bệnh viện, thang máy, hầm xe, đường ống kỹ thuật).

Khuôn: nội thất/ghế ngồi 2048x1024 (góc nhìn thứ nhất, mắt 1,15–1,20 m), thiết bị rời 1024x1024,
tấm nền camera 2048x1152. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 700. Buồng lái máy bay phản lực
*2,20 × 1,60 × 1,30 m · khớp mục 121, xám #4A5058*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
aircraft cockpit 3D game asset, twin-jet airliner flight deck seen from the centre pedestal, two pilot seats in dark grey leather #2E3238, matte charcoal glareshield #26292E, six rectangular glass displays in a 2+2+2 layout with blank dark screens, overhead panel with four rows of guarded toggles and rotary knobs, centre pedestal with two thrust levers, a flap lever and a speedbrake lever, rudder pedals, side consoles with radio and audio panels, circuit-breaker panel behind the captain seat, stowed sun visors, two sliding side windows, windshield frames in matte black, cockpit door with a small porthole, wear: scuffed armrests, faded seat bolsters, dust film on the glareshield, coffee ring on the pedestal, first-person cockpit view from the pilot seat, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 701. Buồng lái máy bay nhỏ
*1,20 × 1,10 × 1,25 m · khớp mục 120, nội thất nâu #8A7761*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
light aircraft cockpit 3D game asset, high-wing two-seat cabin seen from the pilot seat, two seats in sun-faded tan vinyl #8A7761, black crackle-finish instrument panel with six round analogue gauges and a magnetic compass on the glareshield, control yoke with a moulded grip and a centre button, push-pull throttle and mixture knobs, flap switch, radio stack with blank dark faceplates, cream perforated headliner, thin brown carpet #3F3A33, door trim with a sagging map pocket, windshield centre strut in matte black, wear: cracked seat seams, sticky dust on the panel, cigarette burn on the passenger seat, sun-bleached fabric, first-person cockpit view from the pilot seat, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 702. Buồng lái trực thăng
*1,50 × 1,60 × 1,35 m · khớp mục 118/119, olive #4B5A3C*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
helicopter cockpit 3D game asset, light utility helicopter flight deck seen from the pilot seat, two seats in olive canvas #4B5A3C with five-point harnesses, cyclic stick with grip buttons and a guarded trigger, collective lever on the left with a twist throttle and a switch box, anti-torque pedals, overhead console with three breaker rows, centre pedestal with a small multifunction display and a radio head, bubble canopy frame in matte black, chin window in the floor, two roof windows, wear: faded canvas, scuffed floor plate, oil smudge along the canopy seal, chipped paint on the collective head, first-person cockpit view from the pilot seat, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 703. Khoang chở trực thăng vận tải
*3,20 × 1,90 × 1,80 m · khớp mục 477, olive #5A6349*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
transport helicopter cabin 3D game asset, troop compartment seen from the open rear ramp, two folding canvas benches along the walls for eight seats, red cargo straps hanging from the roof rails, floor with flush tie-down rings and worn non-slip strips, sliding door on each side with a gun-mount ring and a stowed M60 cradle, roof hatch with a hoist ring, ribbed soundproofing panels in olive #5A6349, grab handles, first-aid box, fire extinguisher, wear: dried mud on the floor, chipped paint on the ribs, frayed strap ends, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 704. Nội thất xe tăng
*2,40 × 2,00 × 1,60 m · khớp mục 478/115, xanh rêu #3B4A34*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
main battle tank fighting compartment 3D game asset, interior seen from the loader's hatch, driver's station forward with three periscopes and two steering tillers, gunner's station with a binocular sight and a two-axis control handle, commander's cupola with six vision blocks and a hatch ring, rotating turret basket floor, 120 mm breech ring behind the mantlet with a sliding wedge block and a recoil guard, ready rack with six vertical rounds, coaxial machine-gun feed tray, fire-suppression bottle, intercom box, hydraulic lines and cable loom along the walls, interior in dark green #3B4A34 with grey primer patches, wear: oil film, brass shavings in the basket, scuffed hatch ring, dusty periscope glass, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 705. Nội thất xe bọc thép chở quân
*3,00 × 1,80 × 1,60 m · khớp mục 114, trắng xám #C8CCC6*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
armoured personnel carrier troop compartment 3D game asset, interior seen from the open rear ramp, two padded benches for six along the walls, grey-white #C8CCC6 ribbed wall panels, four rifle ports with sliding armoured covers, two roof hatches with periscopes, ramp with a hydraulic cylinder and a manual pump handle, stowed pioneer tools on the ceiling clips, fire extinguisher, ammo boxes under the benches, wear: boot scuffs on the floor, rust streaks at the panel edges, mud on the ramp lip, chipped paint around the ports, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 706. Góc nhìn camera UAV
*16:9 · khớp mục 472/475, tấm nền cho màn hình UAV*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
aerial drone camera plate 3D game asset, wide 110 degree rectilinear lens, oblique view 35 degrees down from 90 m over a mixed downtown district, mid-rise concrete blocks with rooftop water tanks, two glass towers, a six-lane road with sparse traffic, palms along the median, corrugated roofs, hazy late-afternoon light, slight corner falloff and a cool green-blue cast of a machine camera, light sensor grain, no HUD and no overlay of any kind, single flat image with no frame border, no caption strip, no letterbox, no lens flare, 2048x1152, no text, no labels, no arrows, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, no checkerboard, no dot grid
```

### 707. HUD màn hình UAV
*16:9 · texture trong suốt key từ nền trắng, xanh #A6E22E*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
drone HUD texture 3D game asset, orthographic flat layout on a pure white background, thin 3 px olive-green #A6E22E vector lines: centre crosshair with range ticks, horizon ladder with three rungs, left speed bar and right altitude bar with tick marks but no digits, battery arc at the top right, GPS diamond, four waypoint chevrons, corner brackets, one target box with cross ticks and a corner notch, a small compass ribbon with tick marks only, flat 2D shapes with no glow and no scanlines, all elements inside a 10 percent safe margin, 2048x1152, no words, no letters, no numbers, no labels, no arrows beyond the chevrons, no watermark, no signature, no checkerboard, no dot grid, no gradient background
```

### 708. Bảng điều khiển tên lửa
*1,20 × 0,80 × 1,10 m · khớp mục 481/483, xám rêu #5E6B5A*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
missile launch control console 3D game asset, freestanding steel desk unit, sloped face with two 4:3 CRT screens recessed behind glass and switched off, a launch button under a red flip cover, a key switch, sixteen toggle switches in two rows, one rotary selector with a knurled knob, an intercom handset in a side cradle, louvred cooling vents, cable trunking at the base, panel frames stencilled without lettering, chipped olive-grey #5E6B5A paint with bare metal edges, wear: scratched armrest, dusty screen glass, coffee ring on the desk, worn red cover hinge, 3/4 view, pivot at the base, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 709. Trạm điều khiển UAV mặt đất
*1,80 × 0,80 × 0,95 m · khớp mục 472/473, đen #3C4038*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
unmanned aircraft ground control station 3D game asset, folding field desk 1,80 m wide, two 24-inch monitors side by side on tilt-and-swivel mounts with blank dark screens, joystick and throttle quadrant bolted to the desk edge, rugged keyboard in a rubber tray, laptop on a riser, headset on a hook, intercom box, coiled cable loom taped to the desk leg, transit case on the floor, matte black and olive #3C4038 finish, wear: scuffed corners, gaffer tape on a cable, dust film on the monitor bezels, worn joystick boot, 3/4 view, pivot at the base, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 710. Tay cầm điều khiển UAV
*0,30 × 0,12 × 0,18 m · khớp mục 472, đen nhám #23262A*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
drone handheld controller 3D game asset, twin-stick gamepad body in matte black plastic #23262A, folding phone clamp on the top rail, two short whip antennas, four shoulder buttons, textured rubber grips, battery hatch with a quarter-turn latch, lanyard loop, status LED, worn thumbstick caps, faint seam lines and moulding marks, no branding anywhere, 3/4 view, pivot at the base, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 711. Nội thất phòng khách nhà phố
*5,0 × 4,2 × 3,1 m · khớp mục 208, gạch kem #D9D2C4*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
Vietnamese townhouse living room 3D game asset, 5,0 × 4,2 × 3,1 m, 600 mm floor tiles in cream #D9D2C4 with grey grout, walls in warm off-white #E8E4DA with a painted dado line at 0,9 m, fluorescent tube fixture on a plain ceiling, three-seat fabric sofa in grey-brown #7A6E60, low tea table with a smoked glass top and a steel frame, 43-inch TV on a plywood console, ceiling fan with three blades, aluminium sliding window with a steel security grille, stack of plastic stools, pedestal fan in the corner, scuffed skirting, water stain in one ceiling corner, wear: chipped tile grout, faded curtain hem, worn sofa arms, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 712. Nội thất phòng ngủ căn hộ
*3,6 × 3,2 × 2,8 m · khớp mục 206, sàn laminate #8A6A4A*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
apartment bedroom 3D game asset, 3,6 × 3,2 × 2,8 m, laminate floor in walnut #8A6A4A, walls in cool white #EDEDE9, double bed 1,6 × 2,0 m with a foam mattress, grey cotton duvet and two pillows, plywood headboard, two-drawer bedside table with a small lamp, sliding-door wardrobe with a mirror panel, wall-mounted split air conditioner with a yellowed case, compact desk and a plastic stool, blackout curtain on a bent rail, cable bundle under the desk, wear: scuffed floor near the door, dust on the fan louvres, faded duvet edge, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 713. Nội thất bếp
*3,0 × 2,4 × 2,8 m · khớp mục 208/211, gạch trắng #E6E4DC*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
domestic kitchen 3D game asset, 3,0 × 2,4 × 2,8 m, splashback in 300 mm white tiles #E6E4DC with a grey grout grid, two-burner gas stove on a stainless counter, range hood with a grease-stained filter, single-bowl stainless sink with a dish rack, wall cabinets in laminate woodgrain, fridge in the corner with magnets and a chipped handle, rice cooker on a side table, hanging utensil rail, exposed plumbing under the sink, tiled floor with a floor drain, wear: grease film near the hood, scorched burner ring, water stain under the sink, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 714. Nội thất phòng tắm
*2,2 × 1,6 × 2,6 m · khớp mục 208, gạch trắng ngà #E4E2D8*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
domestic bathroom 3D game asset, 2,2 × 1,6 × 2,6 m, walls in 300 × 600 mm off-white tiles #E4E2D8 with a thin blue border course at 1,4 m, anti-slip grey floor tiles #9EA29C, shower corner with a chrome rail and a milky curtain, mirror cabinet over a ceramic washbasin, wall-hung toilet with a dual-flush plate, small instant water heater, stainless floor drain, towel ring with a faded towel, wear: mildew in the grout, limescale on the tap and the glass, chipped tile at the door threshold, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 715. Nội thất cửa hàng tiện lợi
*8 × 10 × 3,2 m · khớp mục 226, sàn xám #6E7276*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
convenience store interior 3D game asset, 8 × 10 × 3,2 m sales floor, four gondola shelving runs with blank price rails, drinks fridge bank along the back wall with lit shelves, chest freezer with sliding glass lids, checkout counter with a barcode scanner, a card terminal and a receipt printer, cigarette rack behind the counter, security mirror in a corner, grid ceiling with fluorescent panels, grey vinyl floor #6E7276 with scuff marks, wire basket stack, wear: scuffed floor at the door, dusty shelf edges, taped shelf corner, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 716. Nội thất sàn văn phòng
*18 × 14 × 3,4 m · khớp mục 204, thảm xám #5C6168*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
open-plan office floor 3D game asset, 18 × 14 × 3,4 m, twelve workstations in two rows with grey fabric partitions at 1,3 m, black mesh task chairs, dual monitors with blank dark screens, keyboards and mouse mats, a glazed meeting room with an aluminium frame and a frosted band, raised floor in grey carpet tiles #5C6168, grid ceiling with 600 mm light panels and four ducted diffusers, cable trays above the ceiling grid, printer corner with paper boxes, wear: coffee stains on the carpet, scuffed partition tops, worn chair armrests, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 717. Nội thất nhà hàng
*10 × 8 × 3,2 m · khớp mục 213, gạch #B9A88F*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
restaurant interior 3D game asset, 10 × 8 × 3,2 m dining room, eight square tables with laminate tops and steel pedestal bases, bentwood chairs with woven seats, a banquette along one wall in dark green vinyl #3E5A4A, service counter with a glass display case, open pass to the kitchen with a stainless shelf, pendant lamps over each table, two ceiling fans, tiled floor in terracotta #B9A88F, blank framed boards on the wall, wear: ring marks on the tables, scuffed chair legs, worn floor grout, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 718. Nội thất quán bar
*12 × 8 × 3,4 m · khớp mục 229, gỗ tối #4A3A2E*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
bar interior 3D game asset, 12 × 8 × 3,4 m, L-shaped bar with a zinc top and a dark timber front #4A3A2E, six stools with cracked vinyl seats, back-bar shelving with bottles and glasses, two draught taps, an unlit blank neon frame on the wall, pool table with a worn green felt, dartboard, two ceiling fans, pendant lamps over the bar, sticky dark floorboards, mirrored strip behind the bottles, wear: ring stains on the zinc, chipped stool edges, worn felt at the pockets, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 719. Hành lang bệnh viện
*3,0 m rộng × 2,8 m cao · khớp mục 233, sàn xám #A6AAA4*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
hospital corridor 3D game asset, 3,0 m wide and 2,8 m high, seamless vinyl floor in matte grey #A6AAA4 with a darker border strip, walls in pale green-white #E7EBE5 with a steel handrail and a bumper rail on both sides, doors with tall vision panels and kick plates, patient trolley, folded wheelchair, linen cart, nurse-call panels, continuous linear ceiling light, a wall-mounted hand-sanitiser dispenser, wear: scuff marks at trolley height, dull polish near the doors, a chipped door edge, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 720. Sảnh đồn cảnh sát
*8 × 6 × 3,2 m · khớp mục 234, gạch #A8AAA6*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
police station front desk area 3D game asset, 8 × 6 × 3,2 m, counter with a laminate top and a glass screen with a speaking slot, waiting bench in blue vinyl, holding bench with a steel rail, blank noticeboard with pinned empty sheets, security door with a keypad, dome camera in the ceiling corner, grey tile floor #A8AAA6, fluorescent ceiling troffers, radiators, a stack of plastic chairs, wear: scuffed counter edge, taped noticeboard corner, dull floor polish, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 721. Nội thất nhà kho
*30 × 20 × 9 m · khớp mục 219/247, bê tông #8E918C*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
warehouse interior 3D game asset, 30 × 20 × 9 m, steel portal frame with visible purlins, four pallet racking runs three levels high with mixed cartons and shrink-wrapped pallets, two roller dock doors and one open bay, high-bay lights on trunking, forklift parked by the racking, painted yellow floor lines and bay numbers as plain stripes with no digits, sealed concrete floor #8E918C, dust in the air near the lights, wear: tyre marks, oil patch under the forklift, dented rack upright, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 722. Nội thất hầm để xe
*36 × 24 × 2,7 m · khớp mục 237/254, bê tông #7E817C*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
underground parking level 3D game asset, 36 × 24 × 2,7 m, two rows of 24 marked bays with plain painted lines and no digits, square columns wrapped in yellow-and-black hazard stripes up to 1,2 m, low concrete ceiling with galvanised ductwork and sprinkler pipe, dim fluorescent tube fixtures, ramp mouth with a light spill, tyre marks and oil stains on the floor #7E817C, a wheel stop at each bay, speed bump, wear: chipped column corners, dust film on the pipes, puddles near the ramp, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 723. Cabin thang máy
*1,6 × 1,5 × 2,4 m · khớp mục 266, inox #B7BAB9*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
elevator car interior 3D game asset, 1,6 × 1,5 × 2,4 m, brushed stainless wall panels #B7BAB9 with a mirror on the rear wall, steel handrail on three sides, button panel with twelve round buttons and a key switch, blank floor indicator above the door with no digits, speckled vinyl floor with an aluminium threshold, ceiling with a frosted light diffuser and a small fan, door with a rubber bumper edge and a narrow vision strip, wear: fingerprint smears on the steel, scuffed floor at the threshold, dull handrail, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 724. Giếng thang máy và phòng máy
*2,2 × 2,0 × 20 m · khớp mục 266, thép xám #6B6E6A*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
elevator shaft and machine room 3D game asset, 2,2 × 2,0 m shaft 20 m tall with two guide rails, a counterweight running in its own frame, six hoist ropes, car frame with guide shoes, buffer springs and a pit ladder at the bottom, landing doors on four levels, machine room above with a traction motor and brake drum, a controller cabinet with louvred doors, cable bundle and governor, concrete walls #8A8D88 and steel parts in #6B6E6A, wear: grease streaks on the rails, dust on the cabinet, oil stain in the pit, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 725. Cầu thang bộ
*1,4 m rộng · khớp mục 208/204, bê tông #A9ABA6*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
concrete stairwell 3D game asset, 1,4 m wide flight with eighteen 170 mm risers, half-landing, steel handrail on both sides bolted to the wall, painted wall with a two-tone dado at 1,1 m (#D8D6CE above, #8E9A8A below), landing window with a steel frame, floor in grey terrazzo #A9ABA6, exposed conduit and a light fixture on the landing, wear: scuffed stair nosing, dust in the corners, chipped paint at the handrail anchors, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 726. Sân thượng
*12 × 9 m · khớp mục 206/208, bê tông #9A9C96*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
rooftop deck 3D game asset, 12 × 9 m, concrete slab with a grey waterproof membrane #9A9C96, 1,1 m parapet with a metal coping, stair penthouse with a steel door, 2 m³ plastic water tank on a steel stand, two air-conditioner condensers with fan guards, antenna mast with three directional panels, satellite dish, floor drain and a hose bib, cable runs in conduit, wear: cracked membrane, rust streaks on the stand, moss in the drain corner, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 727. Trong két sắt ngân hàng
*4 × 3 × 2,8 m · khớp mục 255/297, thép #5E6468*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
bank vault interior 3D game asset, 4 × 3 × 2,8 m, steel-lined walls #5E6468 with a riveted panel grid, deposit-box wall with 72 numbered plates whose digits are blank embossed rectangles, cash cart with three canvas bags, counting table with a note banding machine, cage door with a barred inner gate, fluorescent fixture with a wire guard, floor in polished concrete, wear: scuffed steel near the door, dust on the box handles, worn cart wheels, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 728. Hành lang kỹ thuật chui
*1,2 × 1,0 m tiết diện · khớp mục 254/249, bê tông #83867F*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh*

```
service crawlspace and maintenance tunnel 3D game asset, 1,2 × 1,0 m cross-section, raw concrete walls and ceiling #83867F, cable trays with three bundled looms on one wall, two steel pipe runs with valves on the other, junction boxes with open lids, a sump with a steel grate and a trickle of water, single caged bulkhead bulb every 6 m, loose conduit, dust and cobwebs in the corners, wear: rust at the brackets, damp stain along the floor, chipped concrete at the junction, first-person interior view, eye level 1,20 m, wide 70 degree lens, correct 1:1 scale, separate fixtures and moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no scenery outside the openings, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```
