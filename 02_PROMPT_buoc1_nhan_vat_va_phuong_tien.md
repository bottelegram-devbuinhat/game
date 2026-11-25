# BƯỚC 1 — Nhân vật và phương tiện (kể cả nội thất xe)

Bước 1/5 — 31 mục: 10 nhân vật (001–061) + 21 phương tiện & nội thất xe (062–699).

Đọc `00_PROMPT_TONG.md` trước file này nếu chưa đọc: nó có quy ước nền trắng, khung ảnh, cách đặt
tên, gate kiểm và luật không bịa kết quả. File này chỉ chứa nội dung prompt — copy nguyên khối, không
rút gọn, không gộp hai mục vào một ảnh.

**Nguồn:** `01_nhan_vat.md` (file cũ, đã gộp vào bước này), `02_phuong_tien.md` (file cũ, đã gộp vào bước này). Gộp nguyên văn, số hiệu mục giữ đúng như bản gốc.

Đếm của bước này (phải khớp trước khi đóng zip):

```bash
grep -h '^### ' prompts/02_buoc1_nhan_vat_phuong_tien.md | wc -l   # phải ra 31
```

---

## Nhân vật — 10 mục còn lại

*Nguồn: `01_nhan_vat.md` (file cũ, đã gộp vào bước này) — 10 mục, copy nguyên khối bên dưới.*

*Ghi chú: file này chỉ còn mục chưa đạt. 565 prompt của ảnh đã duyệt đã xoá khỏi danh sách đang làm (bản đầy đủ 683 mục vẫn nằm trong git, commit `d332e04`) — đừng sinh lại các mục đó.*

Khuôn: 1536x1024 cho cả ba hướng, 512x512 cho chi tiết cơ thể rời. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 001. Dude mặc định
*1,80 m · bộ gốc*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, adult male protagonist, short dark brown hair, light stubble, athletic build, plain white crew-neck t-shirt with cotton folds, faded blue straight-leg jeans with belt loops and rivets, white low-top sneakers with rubber toe caps, 1,80 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 008. Dude đồ bơi
*1,80 m*
*Lỗi lô trước: nhân vật chiếm quá ít khung (3 hướng đứng ~1/3 chiều cao, thừa nền trắng)*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, adult male, short dark hair, bare chest, navy swim shorts with drawstring and side stripe, black slide sandals, 1,80 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 022. Cảnh sát đặc nhiệm
*1,82 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, SWAT officer, matte black helmet with raised visor, black balaclava, bulky plate-carrier vest with magazine pouches and radio, elbow pads, knee pads, black tactical trousers, combat boots, 1,82 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 029. Lính nghĩa vụ
*1,80 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, conscript soldier, olive-drab camouflage uniform with rolled sleeves, steel helmet with chin strap, webbing belt with two ammunition pouches, canvas boots, 1,80 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 032. Lễ tân khách sạn
*1,72 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, female hotel receptionist, dark hair in a low bun, white fitted vest over a white shirt, black bow tie, small blank name tag, black pencil skirt, low heels, 1,72 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 034. Đầu bếp
*1,75 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
character reference sheet, 3D game asset, stylized semi-realistic cartoon proportions, male chef, white double-breasted jacket with cloth buttons, black-and-white check trousers, waist apron, tall pleated chef hat, black non-slip shoes, 1,75 m tall, full-body character sheet with front, side and back views in one horizontal row, neutral A-pose, arms slightly away from the torso, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 053. Đầu đội mũ bảo hiểm
*0,28 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
3D game asset, male head wearing an open-face motorcycle helmet, visor flipped up, chin strap, matte black shell with a thin white stripe, eyes visible, neutral expression, 0,28 m tall, isolated single body part, no other body parts in frame, front view with a slight side turn, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 057. Bàn tay trái mở
*0,19 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh; chữ callout/ghi chú/số đo bake vào ảnh*

```
3D game asset, left hand, palm forward, five fingers relaxed and slightly spread, visible knuckle creases and short nails, 0,19 m long, isolated single body part, no other body parts in frame, front view with a slight side turn, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 060. Chân quần dài
*0,85 m*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
3D game asset, right leg from hip to foot, straight-leg trousers with pressed crease and hem stitching, lace-up leather boot with welted sole, 0,85 m long, isolated single body part, no other body parts in frame, front view with a slight side turn, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 061. Chân quầnngắn
*0,85 m*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
3D game asset, right leg from hip to foot, knee-length cargo shorts with a side pocket, low-top sneaker with white sole, 0,85 m long, isolated single body part, no other body parts in frame, front view with a slight side turn, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, subject fills 90% of the frame height, 1536x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

---

## Phương tiện — 21 mục còn lại

*Nguồn: `02_phuong_tien.md` (file cũ, đã gộp vào bước này) — 21 mục, copy nguyên khối bên dưới.*

*Ghi chú: file này chỉ còn mục chưa đạt. 565 prompt của ảnh đã duyệt đã xoá khỏi danh sách đang làm (bản đầy đủ 683 mục vẫn nằm trong git, commit `d332e04`) — đừng sinh lại các mục đó.*

Khuôn: 2048x1024 cho xe, 1024x1024 cho chi tiết nội thất. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 062. Sedan
*4,60 × 1,80 × 1,45 m · bánh R 0,32, màu #B8BCC4*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
vehicle 3D game asset, 4-door sedan, smooth painted steel body in #B8BCC4, chrome window trim and door handles, body-colour bumpers, black grille with a chrome bar, clear headlights with amber indicators, side mirrors, four opening doors with panel gaps, 4,60 × 1,80 × 1,45 m, wheel radius 0,32 m, five-spoke alloy wheels with black tyres, 3/4 front view, four wheels modelled as separate meshes, correct 1:1 scale, crisp hard-surface topology, visible panel gaps, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, the whole vehicle fits inside the frame with a small even margin
```

### 064. Muscle GT
*4,80 × 1,95 × 1,35 m · bánh R 0,34, #C0392B*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
vehicle 3D game asset, american muscle car, long hood, fastback roofline, matte black hood scoop, wide rear tyres on deep-dish wheels, chrome bumpers, quad round headlights, twin exhaust tips, body colour #C0392B, 4,80 × 1,95 × 1,35 m, wheel radius 0,34 m, 3/4 front view, four wheels modelled as separate meshes, correct 1:1 scale, crisp hard-surface topology, visible panel gaps, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, the whole vehicle fits inside the frame with a small even margin
```

### 077. Taxi
*4,60 × 1,80 × 1,45 m · bánh R 0,32, #63DB4C*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
vehicle 3D game asset, city taxi sedan, two-tone body with #63DB4C lower panels, black-and-white checker stripe along the doors, lit roof sign box, blank door panel for livery, chrome trim, 4,60 × 1,80 × 1,45 m, wheel radius 0,32 m, 3/4 front view, four wheels modelled as separate meshes, correct 1:1 scale, crisp hard-surface topology, visible panel gaps, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, the whole vehicle fits inside the frame with a small even margin
```

### 086. Xe tải nhỏ
*6,20 × 2,20 × 2,90 m · bánh R 0,42*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh; chữ callout/ghi chú/số đo bake vào ảnh*

```
vehicle 3D game asset, small box truck, white cab with black grille and steel bumper, plain cargo box with vertical ribs, rear roller shutter, side marker lights, mud flaps, 6,20 × 2,20 × 2,90 m, wheel radius 0,42 m, six wheels, 3/4 front view, four wheels modelled as separate meshes, correct 1:1 scale, crisp hard-surface topology, visible panel gaps, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, the whole vehicle fits inside the frame with a small even margin
```

### 110. Xe bán kem
*4,80 × 1,90 × 2,30 m*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
vehicle 3D game asset, ice cream van, rounded cab, large side serving window with awning, blank menu panels, striped lower skirt, roof-mounted fibreglass ice cream cone, small chimney vent, 4,80 × 1,90 × 2,30 m, wheel radius 0,34 m, 3/4 front view, four wheels modelled as separate meshes, correct 1:1 scale, crisp hard-surface topology, visible panel gaps, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo, the whole vehicle fits inside the frame with a small even margin
```

### 684. Nội thất sedan
*1,40 × 0,60 × 0,90 m · khớp mục 062, màu #B8BCC4*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, sedan cockpit seen from the driver seat, soft-touch dashboard in #B8BCC4 with a dark grey upper pad, analogue instrument cluster behind the wheel, rectangular centre screen, three rotary climate knobs, gear lever, handbrake, two fabric front seats, door cards with window switches, rear-view mirror, plain glass windshield and side windows, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 685. Nội thất xe thể thao
*1,40 × 0,55 × 0,85 m · khớp mục 064/075, màu #C0392B*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, sports car cockpit seen from the driver seat, low dashboard with a hooded instrument binnacle, three auxiliary gauges on the centre stack, flat-bottom steering wheel, bucket seats with red stitching, short throw shifter, aluminium pedals, door pulls, windshield with a plain glass surface, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 686. Nội thất bán tải
*1,50 × 0,65 × 1,00 m · khớp mục 066/067*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, pickup truck cockpit seen from the driver seat, upright dashboard with a square instrument cluster, column-mounted shifter, bench seat with three headrests, rubber floor mats, grab handle on the A-pillar, manual window cranks, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 687. Nội thất xe tải nhỏ
*1,60 × 0,70 × 1,10 m · khớp mục 086*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, light truck cabin seen from the driver seat, wide flat dashboard with a tachograph slot, big four-spoke steering wheel, air-brake knobs, bench seat for three, door pockets, tall windshield, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 688. Nội thất xe buýt
*1,80 × 0,80 × 1,20 m · khớp mục 084*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, city bus driver area, wide dashboard with a fare box beside the seat, ticket printer, large steering wheel, electrically operated door switches, passenger handrails and a step well behind the driver, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 689. Nội thất xe cảnh sát
*1,40 × 0,65 × 0,95 m · khớp mục 079*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, police cruiser cockpit seen from the driver seat, dashboard with a radio head unit and a light-bar controller, laptop mount on the console, partition cage behind the front seats, shotgun rack between the seats, push bumper switch, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 690. Nội thất taxi
*1,40 × 0,60 × 0,90 m · khớp mục 077*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, taxi cockpit seen from the driver seat, dashboard with a windscreen-mounted fare meter, roof-sign switch, two-way radio, worn vinyl seats, partition glass behind the driver, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 691. Nội thất xe cứu thương
*1,60 × 0,70 × 1,15 m · khớp mục 083*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, ambulance cab seen from the driver seat, dashboard with a radio and siren controller, medical cabinet behind the seats, oxygen bottle bracket, stretcher rail visible in the rear compartment, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 692. Nội thất xe cứu hỏa
*1,80 × 0,75 × 1,20 m · khớp mục 082*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
vehicle interior 3D game asset, fire truck cab seen from the driver seat, dashboard with air-brake valves and pump gauges, large steering wheel, crew seats with SCBA brackets, radio handset, vertical grab rails, first-person driving view, eye level 1,15 m, symmetrical composition, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background visible only outside the glass, single tone, no gradient, no checkerboard, no dot grid, no scenery, no horizon, 2048x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 693. Ghi-đông xe máy
*0,75 × 0,55 × 0,45 m · khớp mục 068/069/070*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, motorcycle handlebar assembly seen from the rider position, straight bar with rubber grips, brake and clutch levers, two round mirrors on stalks, small gauge cluster with two dials, headlight shell, ignition key, top of the fuel tank below, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 694. Táp-lô rời
*1,40 × 0,45 × 0,35 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, loose car dashboard shell, glovebox door, two round air vents, radio slot, instrument binnacle with no gauges, mounting tabs, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 695. Ghế xe
*0,55 × 0,60 × 1,05 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, car driver seat, fabric upholstery with side bolsters, adjustable headrest, reclining hinge, two steel floor rails, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 696. Vô lăng
*Ø 0,36 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, three-spoke steering wheel, leather rim, horn pad with a blank centre, two column stalks, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 697. Ốp cửa trong xe
*0,95 × 0,60 × 0,12 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, car inner door card seen from inside the cabin, armrest, door handle, window switch panel, speaker grille, map pocket, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 698. Gương chiếu hậu trong xe
*0,25 × 0,08 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, interior rear-view mirror, rectangular glass, black plastic housing, day-night flip lever, ball joint stem, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 699. Cần số
*0,30 m*
*Lỗi lô trước: mục mới, chưa từng sinh ảnh — bổ sung khoang nội thất*

```
3D game asset, floor gear shifter, leather boot with stitching, round knob with a blank top, chrome shaft, first-person reference view, correct 1:1 scale, separate moving parts, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```
