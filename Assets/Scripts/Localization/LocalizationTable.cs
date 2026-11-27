using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Localization
{
    // Key-based EN + VI table; falls back requested -> English -> raw key (§1.2 & §1.8).
    public static class LocalizationTable
    {
        private static readonly Dictionary<string, (string en, string vi)> Entries = BuildEntries();

        public static string Get(string key, LanguageCode language)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (!Entries.TryGetValue(key, out var pair))
                return key;
            if (language == LanguageCode.Vietnamese && !string.IsNullOrEmpty(pair.vi))
                return pair.vi;
            if (!string.IsNullOrEmpty(pair.en))
                return pair.en;
            return key;
        }

        public static bool ContainsKey(string key) => !string.IsNullOrEmpty(key) && Entries.ContainsKey(key);

        private static Dictionary<string, (string en, string vi)> BuildEntries()
        {
            return new Dictionary<string, (string, string)>
            {
                ["ui.title"] = ("DUDE CHAOS WARS", "DUDE CHAOS WARS"),
                ["ui.tagline"] = ("No rules. Just dude.", "Không luật lệ. Chỉ có Dude."),
                ["ui.hud.health"] = ("HP", "Máu"),
                ["ui.hud.armour"] = ("ARMOUR", "Giáp"),
                ["ui.hud.wanted"] = ("WANTED", "Truy nã"),
                ["ui.hud.retry"] = ("Press R to Retry", "Nhấn R để thử lại"),
                ["ui.pause.resume"] = ("Resume", "Tiếp tục"),
                ["ui.pause.save"] = ("Save Game", "Lưu game"),
                ["ui.pause.quit"] = ("Quit", "Thoát"),

                // 12 districts (§2.5)
                ["ui.district.airport"] = ("Tan Binh Airfield", "Sân bay Tân Bình"),
                ["ui.district.downtown"] = ("Central District", "Quận Trung Tâm"),
                ["ui.district.farm"] = ("Outskirt Paddies", "Đồng Ngoại Ô"),
                ["ui.district.hotel"] = ("Hai Dang Hotel Strip", "Khu Khách Sạn Hải Đăng"),
                ["ui.district.market"] = ("Ben Thanh Market", "Chợ Bến Thành"),
                ["ui.district.military"] = ("North Garrison", "Doanh Trại Phía Bắc"),
                ["ui.district.port"] = ("Saigon Port 7", "Cảng Sài Gòn 7"),
                ["ui.district.residential"] = ("Tube-House Alleys", "Khu Hẻm Dân Cư"),
                ["ui.district.restaurant"] = ("Night Food Street", "Phố Nhậu Đêm"),
                ["ui.district.secret"] = ("Underground Vault", "Hầm Ngầm Bí Mật"),
                ["ui.district.showroom"] = ("Dude Motors Row", "Phố Showroom Xe"),
                ["ui.district.slums"] = ("Canal Shanties", "Xóm Kênh Ổ Chuột"),

                // 10 radio segments (§2.7)
                ["radio.01"] = ("Six AM, gridlock on Ben Bridge. Like every day.", "Sáu giờ sáng, kẹt xe ở cầu Bến. Như mọi ngày."),
                ["radio.02"] = ("Police say security improved. Police also say nothing happened.", "Cảnh sát cho biết tình hình an ninh đã được cải thiện. Cảnh sát cũng cho biết không có vụ nào."),
                ["radio.03"] = ("32 degrees, 80 humidity, nobody remembers the last blackout.", "Hôm nay 32 độ, độ ẩm 80, và không ai nhớ lần cuối thành phố này mất điện."),
                ["radio.04"] = ("A driver called saying he is being chased. Unverified. Good luck.", "Một tài xế gọi tới nói rằng hắn đang bị đuổi. Chúng tôi không xác minh được. Chúc may mắn."),
                ["radio.05"] = ("Fuel prices up. Gold up. People cheaper.", "Giá xăng tăng. Giá vàng tăng. Giá người thì giảm."),
                ["radio.06"] = ("Market in the slums tonight. Unadvertised, everyone knows.", "Đêm nay ở khu ổ chuột có chợ. Không ai quảng cáo, nhưng ai cũng biết."),
                ["radio.07"] = ("Airport delayed four flights. No reason. There is a reason.", "Sân bay thông báo hoãn bốn chuyến. Không có lý do. Có lý do."),
                ["radio.08"] = ("Hai Dang Hotel changed owners a third time. Nobody saw the new one.", "Khách sạn Hải Đăng vừa đổi chủ lần thứ ba trong năm. Chủ mới cũng chưa thấy mặt."),
                ["radio.09"] = ("People ask why I skip crime news. Because criminals listen to me.", "Có người hỏi vì sao tôi không đọc tin tội phạm. Vì tội phạm là bạn của tôi."),
                ["radio.10"] = ("Next bulletin at seven. If the city is still here.", "Bản tin tiếp theo lúc 7 giờ. Nếu thành phố còn.")
            };
        }
    }
}
