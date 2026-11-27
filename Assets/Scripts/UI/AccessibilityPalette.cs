using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.UI
{
    public struct UiPalette
    {
        public Color primaryYellow;
        public Color dangerRed;
        public Color policeBlue;
        public Color moneyGreen;
        public Color panelBackground;
        public Color bodyText;
    }

    public static class AccessibilityPalette
    {
        public static UiPalette Resolve(ColourBlindMode mode, bool highContrast)
        {
            UiPalette p = new UiPalette
            {
                primaryYellow = new Color(1.00f, 0.78f, 0.07f, 1f), // #FFC812
                dangerRed = new Color(0.92f, 0.22f, 0.22f, 1f),     // #EA3838
                policeBlue = new Color(0.17f, 0.29f, 0.84f, 1f),    // #2C4BD6
                moneyGreen = new Color(0.39f, 0.86f, 0.30f, 1f),    // #63DB4C
                panelBackground = highContrast ? Color.black : new Color(0.08f, 0.09f, 0.11f, 0.88f),
                bodyText = Color.white
            };

            switch (mode)
            {
                case ColourBlindMode.Protan:
                case ColourBlindMode.Deutan:
                    p.dangerRed = new Color(0.90f, 0.50f, 0.00f, 1f);
                    p.moneyGreen = new Color(0.34f, 0.71f, 0.91f, 1f);
                    break;
                case ColourBlindMode.Tritan:
                    p.policeBlue = new Color(0.00f, 0.62f, 0.65f, 1f);
                    p.primaryYellow = new Color(1.00f, 0.85f, 0.55f, 1f);
                    break;
            }
            return p;
        }
    }
}
