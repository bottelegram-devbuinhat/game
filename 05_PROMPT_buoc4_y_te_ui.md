# BƯỚC 4 — Y tế & tiêu dùng, UI & texture

Bước 4/5 — 10 mục: 3 y tế/tiêu dùng (518–534) + 7 UI & texture (536–577).

Đọc `00_PROMPT_TONG.md` trước file này nếu chưa đọc: nó có quy ước nền trắng, khung ảnh, cách đặt
tên, gate kiểm và luật không bịa kết quả. File này chỉ chứa nội dung prompt — copy nguyên khối, không
rút gọn, không gộp hai mục vào một ảnh.

**Nguồn:** `09_y_te_tieu_dung.md` (file cũ, đã gộp vào bước này), `10_ui_texture.md` (file cũ, đã gộp vào bước này). Gộp nguyên văn, số hiệu mục giữ đúng như bản gốc.

Đếm của bước này (phải khớp trước khi đóng zip):

```bash
grep -h '^### ' prompts/05_buoc4_y_te_ui.md | wc -l   # phải ra 10
```

---

## Y tế và tiêu dùng — 3 mục còn lại

*Nguồn: `09_y_te_tieu_dung.md` (file cũ, đã gộp vào bước này) — 3 mục, copy nguyên khối bên dưới.*

*Ghi chú: file này chỉ còn mục chưa đạt. 565 prompt của ảnh đã duyệt đã xoá khỏi danh sách đang làm (bản đầy đủ 683 mục vẫn nằm trong git, commit `d332e04`) — đừng sinh lại các mục đó.*

Khuôn: 1024x1024. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 518. Ống nghe
*0,60 m*
*Lỗi lô trước: chưa có ảnh*

```
3D game prop, stethoscope, black rubber tubing coiled, chrome binaurals with soft eartips, stainless steel chestpiece, 0,60 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 521. Khẩu trang y tế
*0,17 m*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
3D game prop, surgical mask, white pleated three-layer fabric, nose wire, two elastic ear loops, 0,17 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

### 534. Kẹo cao su
*0,08 × 0,05 m*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh*

```
3D game prop, chewing gum pack, small rectangular cardboard box, blank wrapper, one stick pulled half out, 0,08 × 0,05 m, 3/4 view, pivot at the base, correct 1:1 scale, crisp game topology, flat studio lighting, no cast shadow, flat pure white #FFFFFF background, single tone, no gradient, no checkerboard, no dot grid, no floor, no wall, no horizon, 1024x1024, no text, no labels, no arrows, no measurement lines, no numbers on the canvas, no watermark, no signature, no UI overlay, no viewport axis gizmo
```

---

## UI và texture — 7 mục còn lại

*Nguồn: `10_ui_texture.md` (file cũ, đã gộp vào bước này) — 7 mục, copy nguyên khối bên dưới.*

*Ghi chú: file này chỉ còn mục chưa đạt. 565 prompt của ảnh đã duyệt đã xoá khỏi danh sách đang làm (bản đầy đủ 683 mục vẫn nằm trong git, commit `d332e04`) — đừng sinh lại các mục đó.*

Khuôn: đúng kích thước ghi trong prompt, nền trắng để key. Mọi prompt đã có sẵn quy ước nền trắng + cấm chữ/caro, copy nguyên khối.

### 536. Logo trò chơi
*1024 × 512*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh; sai thương hiệu: chữ 'STEEL RUSH ARENA' (game là Dude Chaos Wars)*

```
UI 2D asset, game logo, bold condensed lettering spelling exactly DUDE CHAOS WARS, chrome bevel edge, dark red gradient fill, thick black outline, the words are the only text on the image, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 1024x512
```

### 537. Logo phụ
*512 × 512*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh; chữ placeholder 'SECONDARY • MARK' thay vì hình logo*

```
UI 2D asset, secondary logo mark, circular badge with a stylised fist and one star, flat two-tone, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 512x512
```

### 540. Icon súng trường
*128 × 128*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
UI 2D asset, UI icon, assault rifle in three-quarter side silhouette, flat two-tone with a thin outline, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 128x128
```

### 541. Icon shotgun
*128 × 128*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
UI 2D asset, UI icon, pump shotgun in side silhouette, flat two-tone with a thin outline, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 128x128
```

### 553. Icon âm thanh
*128 × 128*
*Lỗi lô trước: nền caro/dot-grid giả trong suốt bị bake vào ảnh*

```
UI 2D asset, UI icon, loudspeaker with two curved sound waves, flat two-tone, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 128x128
```

### 573. Khung túi đồ
*1024 × 512*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh; chữ tiếng Anh + 'PORTUGUÊS' trong khung túi đồ, lẽ ra khung rỗng*

```
UI 2D asset, inventory panel frame, rounded rectangle with a nine-slice border, slotted grid area, two tabs on the top edge, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 1024x512
```

### 577. Atlas icon 4x4
*1024 × 1024*
*Lỗi lô trước: chữ callout/ghi chú/số đo bake vào ảnh; atlas trộn 2 style (icon phẳng + texture thật), có ô trống*

```
UI 2D asset, icon atlas, 16 flat game icons in a 4x4 grid, one icon per cell, same style and stroke weight, no empty cells: pistol, rifle, shotgun, sniper, grenade, medkit, armour, money, star, lock, map, settings, sound, car, house, food, single flat 2D asset, front orthographic view, crisp clean edges, flat colours, flat pure white #FFFFFF background, single tone, no gradient background, no checkerboard, no dot grid, no shadow, no text, no labels, no arrows, no measurement lines, no watermark, no signature, no extra UI chrome, no viewport axis gizmo, 1024x1024
```
