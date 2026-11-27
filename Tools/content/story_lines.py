#!/usr/bin/env python3
"""Canonical story dialogue, radio segments, character voices, and endings (§2.6 & §2.7).

Every mission has exactly 3 Vietnamese dialogue lines:
  [0] intro
  [1] mid-run / fail banter
  [2] completion payoff

Continuity rules (§2.7):
  - Dude never speaks, not even in text.
  - Ba Bay never lies to him, only omits.
  - Vinh never kills anyone himself.
  - Inspector Lam never takes money.
  - Ti Coi never dies.
  - The box is never opened.
"""

from __future__ import annotations

MISSION_DIALOGUE: dict[str, tuple[str, str, str]] = {
    # Act I. The box (1-5)
    "mis_01_wake_up": (
        "Dậy đi. Thành phố này không chờ ai đâu.",
        "Ra đường đi, đứng nhìn xe mãi à.",
        "Ừ, giờ mày có xe. Đừng làm hỏng nó ngay.",
    ),
    "mis_02_delivery": (
        "Giao cái hộp này tới quán bà Bảy. Đừng mở.",
        "Đừng dừng giữa đường, không ai thích người đứng giữa đường.",
        "Xong. Bà Bảy nhớ mặt mày rồi đấy.",
    ),
    "mis_03_rooftop": (
        "Thằng đó chạy lên mái. Lên theo.",
        "Đừng để nó nhảy trước mày.",
        "Nhảy giỏi đấy. Giờ xuống đi.",
    ),
    "mis_04_car_meet": (
        "Tụi nó họp ở bãi sau. Vào đi.",
        "Đừng khoe xe. Khoe là mất.",
        "Mày vừa ghi tên vào danh sách. Không rút ra được đâu.",
    ),
    "mis_05_showdown": (
        "Tới giờ thanh toán rồi.",
        "Đừng nói nhiều, nó không nghe đâu.",
        "Xong vụ này mày không còn là thằng giao hàng nữa.",
    ),
    # Act II. The errand boy (6-10)
    "mis_06_chop_shop": (
        "Cái xe đó đáng giá hơn khi tháo ra.",
        "Chạy tới xưởng trước khi cảnh sát tới.",
        "Tiền đây. Đừng tiêu hết trong một đêm.",
    ),
    "mis_07_heist": (
        "Ba phút trước khi chuông báo động kêu. Ba.",
        "Lấy tiền, đừng lấy gì khác.",
        "Ra được khỏi đây là mày khác rồi.",
    ),
    "mis_08_getaway": (
        "Cả thành phố đang đuổi theo mày.",
        "Đừng để chúng vây. Cứ chạy.",
        "Mất con trực thăng là mày sống.",
    ),
    "mis_09_night_run": (
        "Cảng chỉ mở ban đêm. Buồn cười thật.",
        "Chất hàng lên, đừng hỏi hàng gì.",
        "Về trước khi trời sáng.",
    ),
    "mis_10_slum_deal": (
        "Gặp người ở khu ổ chuột. Mang theo tiền.",
        "Nó phục kích mày đấy. Chuẩn bị đi.",
        "Không ai phản bội tao hai lần.",
    ),
    # Act III. The climb (11-15)
    "mis_11_airport_heist": (
        "Máy bay đi trong bốn phút. Bốn.",
        "Qua cổng, vào hangar, đừng chạm chuông.",
        "Đường băng trống rồi. Chạy đi.",
    ),
    "mis_12_military_run": (
        "Cái xe đó đáng hơn cả căn nhà mày.",
        "Đừng để chạm ụ súng.",
        "Mất dấu tuần tra là mày về được.",
    ),
    "mis_13_room_302": (
        "Phòng 302. Đừng động vào minibar.",
        "Tìm thứ trong két. Không phải tiền.",
        "Cúp máy là mày phải ra khỏi đó.",
    ),
    "mis_14_rooftop_chase": (
        "Nó lên mái. Tất nhiên rồi.",
        "Bắt được trước khi nó nhảy.",
        "Xuống được là mày còn sống.",
    ),
    "mis_15_showroom_hit": (
        "Bảo hiểm là cho người xin phép.",
        "Lấy chìa, đừng lấy xe trưng bày.",
        "Đạp ga qua cửa kính. Đừng nhìn lại.",
    ),
    # Act IV. Collapse (16-17)
    "mis_16_final_chaos": (
        "Cả thành phố đang tới. Làm cho nó hoành tráng vào.",
        "Giữ sảnh. Đừng để thằng nào lọt.",
        "Sóng cuối cùng. Mày vẫn đứng.",
    ),
    "mis_17_prison_break": (
        "Đừng hỏi tại sao nó ở trong đó. Đưa nó ra.",
        "Cắt hàng rào, đừng cắt dây điện.",
        "Mất dấu xe là mất luôn người.",
    ),
    # Act V. Endgame (18-20)
    "mis_18_army_depot": (
        "Kho đó có thứ không mua được ở đâu.",
        "Chất thùng lên, đừng mở.",
        "Ra trước khi chuông kêu.",
    ),
    "mis_19_hotel_siege": (
        "Chúng biết phòng rồi. Để chúng lên.",
        "Giữ cầu thang. Đừng bỏ tầng.",
        "Qua mái mà đi. Thang máy là bẫy.",
    ),
    "mis_20_endgame": (
        "Một vụ cuối. Rồi thành phố này là của mình.",
        "Gọi hỗ trợ đi, lần này không cần giấu.",
        "Đi bộ ra khỏi đó. Không ai cản.",
    ),
}

RADIO_SEGMENTS: tuple[str, ...] = (
    "Sáu giờ sáng, kẹt xe ở cầu Bến. Như mọi ngày.",
    "Cảnh sát cho biết tình hình an ninh đã được cải thiện. Cảnh sát cũng cho biết không có vụ nào.",
    "Hôm nay 32 độ, độ ẩm 80, và không ai nhớ lần cuối thành phố này mất điện.",
    "Một tài xế gọi tới nói rằng hắn đang bị đuổi. Chúng tôi không xác minh được. Chúc may mắn.",
    "Giá xăng tăng. Giá vàng tăng. Giá người thì giảm.",
    "Đêm nay ở khu ổ chuột có chợ. Không ai quảng cáo, nhưng ai cũng biết.",
    "Sân bay thông báo hoãn bốn chuyến. Không có lý do. Có lý do.",
    "Khách sạn Hải Đăng vừa đổi chủ lần thứ ba trong năm. Chủ mới cũng chưa thấy mặt.",
    "Có người hỏi vì sao tôi không đọc tin tội phạm. Vì tội phạm là bạn của tôi.",
    "Bản tin tiếp theo lúc 7 giờ. Nếu thành phố còn.",
)

CHARACTERS: dict[str, dict[str, str]] = {
    "dude": {
        "name": "Dude",
        "role": "player",
        "voice": "no dialogue, acts only",
        "sample": "",
    },
    "ba_bay": {
        "name": "Bà Bảy",
        "role": "noodle-stall owner at the market, gives the first job, knows everything first",
        "voice": "short, sober, impatient",
        "sample": "Đừng mở. Tao không nói lần hai.",
    },
    "ti_coi": {
        "name": "Tí Còi",
        "role": "Dude's cousin, driver, loud, bad at driving",
        "voice": "talks a lot, confidently wrong",
        "sample": "Tao biết đường tắt! …chắc vậy.",
    },
    "vinh_baba": {
        "name": 'Vinh "Baba"',
        "role": "showroom owner, the city's real boss, pays half the police",
        "voice": "polite to the point of chilling",
        "sample": "Tao không giận. Tao chỉ ghi sổ.",
    },
    "inspector_lam": {
        "name": "Inspector Lâm",
        "role": "the only cop who still wants to do it right, and is stuck because of it",
        "voice": "tired, sardonic",
        "sample": "Mày nghĩ tao thích bắt mày à?",
    },
    "captain_khang": {
        "name": "Captain Khang",
        "role": "military depot commander, sells weapons to both sides",
        "voice": "boastful, greedy",
        "sample": "Hàng của tao không có hoá đơn.",
    },
    "mai": {
        "name": "Mai",
        "role": "hotel receptionist, side business selling information",
        "voice": "sweet, charges per sentence",
        "sample": "Phòng 302 à? Anh hỏi giá chưa?",
    },
    "song_fm": {
        "name": "Sóng FM",
        "role": "radio DJ narrating the city between jobs",
        "voice": "unrelated to you, and that is the point",
        "sample": "Sáu giờ sáng, kẹt xe ở cầu Bến. Như mọi ngày.",
    },
}

ENDINGS: dict[str, str] = {
    "Clean": "20 missions done, final wanted level < 3: hands the ledger to Lâm, takes the bounty, leaves.",
    "King": "20 missions done, owns >= 3 armoured vehicles: sits in Vinh's chair; the city changes owner, nothing else changes.",
    "Ash": "20 missions done, destroyed >= 20 civilian vehicles: nobody wins, Downtown loses power for three days, Sóng FM keeps broadcasting.",
}


def validate_story_lines() -> list[str]:
    errors: list[str] = []
    if len(MISSION_DIALOGUE) != 20:
        errors.append(f"Expected 20 missions in MISSION_DIALOGUE, got {len(MISSION_DIALOGUE)}")
    for mid, lines in MISSION_DIALOGUE.items():
        if len(lines) != 3:
            errors.append(f"{mid}: expected 3 dialogue lines, got {len(lines)}")
        for idx, line in enumerate(lines):
            if not line or not line.strip():
                errors.append(f"{mid}[{idx}]: empty dialogue line")
    if len(RADIO_SEGMENTS) != 10:
        errors.append(f"Expected 10 radio segments, got {len(RADIO_SEGMENTS)}")
    if CHARACTERS["dude"]["sample"] != "":
        errors.append("Continuity violation: Dude must have no dialogue")
    return errors


if __name__ == "__main__":
    errs = validate_story_lines()
    if errs:
        for e in errs:
            print(f"ERROR: {e}")
        raise SystemExit(1)
    print(f"story_lines OK: {len(MISSION_DIALOGUE)} missions x 3 lines, {len(RADIO_SEGMENTS)} radio segments")
