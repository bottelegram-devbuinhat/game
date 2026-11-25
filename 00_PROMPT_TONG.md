# PROMPT TỔNG — ĐỌC FILE NÀY TRƯỚC KHI LÀM BẤT CỨ GÌ

Mày là người (hoặc AI) sinh ảnh cho game **Dude Chaos Wars** — sandbox thành phố, ragdoll, low-poly
cartoon, bối cảnh thành phố Việt Nam. File này là **prompt điều phối**: nó nói mày đang thiếu gì,
đọc file nào theo thứ tự nào, kiểm cái gì trước khi báo xong. Nó **không** chứa nội dung prompt —
prompt nằm trong các file bước.

Đọc hết file này một lượt rồi mới mở bước đầu tiên. Không đọc hết mà nhảy vào sinh là sai thứ tự.

---

## 1. Đang thiếu gì

Lô ảnh đầu đã sinh và duyệt: **563 ảnh LẤY**, 113 ảnh phải làm lại, **5 ảnh thiếu** → **118 mục cũ**
phải sinh lại. Cộng thêm 67 mục mới chưa từng có prompt:

- **16 mục** khoang nội thất xe (684–699) — bộ cũ chỉ có ngoại thất, ngồi vào xe là trống trơn.
- **29 mục** nội thất khí tài + buồng lái + không gian chui vào được (700–728).
- **22 mục** công trình viết lại (729–750) — prompt cũ chỉ 14–20 từ nên ảnh ra là hộp kính chung chung.

**Tổng cần sinh: 185 ảnh.** Đếm lại đúng bằng:

```bash
grep -h '^### ' prompts/0[2-6]_buoc*.md | wc -l   # phải ra 185
```

563 ảnh đã duyệt **không sinh lại**. Bản đầy đủ 683 mục cũ vẫn nằm trong git ở commit `d332e04`
(`git show d332e04:prompts/...`) — chỉ mở ra khi cần đối chiếu, không lấy prompt từ đó.

---

## 2. Thứ tự làm — 5 bước, không đảo

Cả pack đánh số 00–08 và số là **thứ tự thực hiện**: `00` file này (điều phối) → `01` brief
rebuild code (dựng project C#) → `02`–`06` năm bước sinh ảnh dưới đây → `07`–`08` prompt Blender
dựng asset 3D. Làm code trước hay ảnh trước không quan trọng, nhưng trong track ảnh thì đúng thứ tự bước.

Mỗi bước là **một phiên làm việc độc lập**: mở đúng một file, paste từng khối, sinh, đóng gói zip,
báo xong. Làm xong bước N mới sang bước N+1.

| Bước | File | Mảng | Mục | Số mục |
|---|---|---|---|---|
| 1 | `02_buoc1_nhan_vat_phuong_tien.md` | Nhân vật + phương tiện & nội thất xe | 001–061, 062–699 | 31 |
| 2 | `03_buoc2_sung_kien_truc_thien_nhien.md` | Súng & phụ kiện, kiến trúc & nội thất, thiên nhiên & động vật | 122–202, 211–750, 310–342 | 72 |
| 3 | `04_buoc3_do_dac_hieu_ung_khi_tai.md` | Đồ đạc đường phố, hiệu ứng, khí tài quân sự | 373–405, 413–466, 472 | 30 |
| 4 | `05_buoc4_y_te_ui.md` | Y tế & tiêu dùng, UI & texture | 518–534, 536–577 | 10 |
| 5 | `06_buoc5_chi_tiet_noi_that.md` | Chi tiết nhỏ, nội thất khí tài / buồng điều khiển | 593–683, 700–728 | 42 |
| | | | **TỔNG** | **185** |

Bước 1 và 5 nặng nhất (31 và 42 mục). Nếu một phiên không kham hết thì cắt trong **cùng một file**,
không trộn hai file bước vào một lượt — trộn là lẫn khuôn ảnh và lẫn cách đặt tên.

---

## 3. Quy ước bắt buộc (đã nhúng trong từng khối, đây là bản tóm để kiểm)

- **Nền trắng phẳng `#FFFFFF`, một tông.** Không dùng "transparent background": máy sinh ảnh không trả
  alpha thật, nền trắng tinh thì key sang alpha sạch.
- **Cấm chữ**: không caption, nhãn, mũi tên chú thích, số đo, watermark, gizmo trục.
- **Cấm caro / dot-grid / gradient bẩn**: đây là lỗi nặng nhất của lô trước (71 ảnh hỏng vì nó).
- **Đúng khung theo loại**:
  | Loại | Khung |
  |---|---|
  | Nhân vật | `1536x1024` cho cả ba hướng, `512x512` cho chi tiết cơ thể rời |
  | Xe | `2048x1024`; chi tiết nội thất xe `1024x1024` |
  | Súng / phụ kiện | `2048x1024`, nhìn nghiêng, nòng hướng phải |
  | Công trình | `2048x2048` |
  | Nội thất, vật thể, khí tài, y tế | `1024x1024` |
  | Hiệu ứng / sprite | dải ngang, mỗi frame một ô vuông, sheet = số frame × cạnh ô |
  | Tấm nền camera | `2048x1152` |
  | UI / texture | đúng số ghi ngay trong khối prompt |
  | Decal (chi tiết nhỏ) | `1024x1024`, nhìn thẳng từ trên |
- **Mỗi khối prompt là một ảnh.** Không gộp hai mục vào một ảnh cho nhanh, không tự rút gọn khối
  prompt, không "cải thiện" mô tả. Khối đã liệt kê sẵn bộ phận + vật liệu + màu + độ mòn — copy nguyên.

---

## 4. Đặt tên và đóng gói

- Ảnh: `NNN Tên.webp` (hoặc `.png`), `NNN` đúng số mục trong tiêu đề khối, `Tên` lấy đúng tên mục.
  Ví dụ mục `### 684. Taplo cửa trước` → `684 Taplo cửa trước.webp`.
- Sai số mục là ảnh mồ côi, phải sinh lại — kiểm số trước khi lưu.
- Zip: một zip mỗi bước, tên `buoc_01.zip` … `buoc_05.zip`. Trong zip chỉ có ảnh, không có file lạ.
- Kèm một dòng log mỗi bước: `buoc_01: 31/31 ảnh, 0 lỗi` hoặc `buoc_03: 27/30, thiếu 415, 421, 455`.

---

## 5. Gate trước khi báo xong một bước

Chỉ báo xong khi tự kiểm được **từng dòng** dưới đây. Không kiểm được thì ghi **NOT RUN**, không
được coi là đạt:

1. **Đủ số**: số ảnh trong zip = số mục của bước (31 / 72 / 30 / 10 / 42).
2. **Đúng số hiệu**: mỗi ảnh mang đúng số mục, không trùng, không thiếu, không thừa số lạ.
3. **Đúng khung**: mở file xem kích thước thật, không tin tên file.
4. **Nền trắng**: không còn caro, không lưới mờ, không đổ bóng nền, không chữ.
5. **Không sinh lại mục đã duyệt**: nếu thấy mình vừa làm một mục nằm ngoài danh sách bước → xoá.

Bước nào trượt gate thì sửa trong bước đó rồi kiểm lại, không đẩy sang bước sau.

---

## 6. Luật cứng

- **Không bịa kết quả.** Chưa mở ảnh ra xem thì không được nói "đạt". Ảnh không ra hình thì ghi rõ
  "mục NNN fail, lý do", không im lặng bỏ qua và không thay bằng ảnh khác.
- **Không tự sửa prompt.** Prompt trong các file bước là bản đã chốt. Thấy sai/không rõ thì ghi lại
  và hỏi người, không tự viết lại rồi sinh.
- **Không đụng phần code.** Brief rebuild code là `01_PROMPT_REBUILD.md` trong cùng thư mục này, dùng riêng. File này
  và các file `BUOC_*` chỉ nói về ảnh.
- **Prompt Blender** (`07_QWEN_BLENDER_PROMPT.md`, bản EN `08_QWEN_BLENDER_PROMPT_EN.md`) là
  đường dựng asset 3D, không phải đường sinh ảnh 2D. Đừng trộn hai đường vào một lượt.

---

## 7. Bắt đầu

1. Đọc xong file này.
2. Mở `02_buoc1_nhan_vat_phuong_tien.md`, paste khối đầu tiên, sinh ảnh đầu tiên.
3. Đi hết bước 1, tự kiểm theo §5, đóng `buoc_01.zip`, báo log.
4. Sang bước 2. Hết bước 5 là hết 185 ảnh.
