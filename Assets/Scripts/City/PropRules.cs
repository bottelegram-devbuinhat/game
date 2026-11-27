using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.City
{
    public enum PropGroup
    {
        StreetFurniture = 0,
        StreetClutter = 1,
        WallDetail = 2,
        RoofAndUtility = 3,
        ShopFront = 4,
        Nature = 5,
        GroundDecal = 6,
        NightSign = 7
    }

    public enum PropRenderTier
    {
        FullShadowed = 0,
        SimplifiedNoShadow = 1,
        FlatDecal = 2,
        Hidden = 3
    }

    [Flags]
    public enum PropFlags
    {
        None = 0,
        BlocksNavigation = 1 << 0,
        Breakable = 1 << 1,
        Grabbable = 1 << 2,
        StayOffRoad = 1 << 3,
        NightOnly = 1 << 4,
        EmitsLight = 1 << 5,
        FlatDecal = 1 << 6
    }

    // Exact 78 prop kinds across 8 groups (§1.4, §1.7, §2.9).
    public enum PropKind
    {
        // StreetFurniture (10)
        StreetLamp = 0, TrafficLight = 1, BusStopShelter = 2, ParkBench = 3, FireHydrant = 4,
        Bollard = 5, MailBox = 6, PhoneBooth = 7, GuardRail = 8, ConcreteBarrier = 9,
        // StreetClutter (12)
        PlasticStoolRed = 10, PlasticTableBlue = 11, TrashBinGreen = 12, CardboardBox = 13, TrafficCone = 14,
        GasCylinder = 15, WaterJug20L = 16, NoodleCart = 17, IceBoxFoam = 18, TireStack = 19,
        WoodenPallet = 20, BroomBucket = 21,
        // WallDetail (10)
        AirConSplitUnit = 22, ElectricMeterBox = 23, TangledCableBundle = 24, DrainPipe = 25, MetalWindowGrille = 26,
        RollUpShutter = 27, BalconyClothesline = 28, ExhaustFanVent = 29, SecurityCamera = 30, AwningCanopy = 31,
        // RoofAndUtility (10)
        StainlessWaterTank = 32, SolarWaterHeater = 33, TvAntenna = 34, SatelliteDish = 35, RoofAccessHatch = 36,
        UtilityPoleConcrete = 37, TransformerDrum = 38, CellTowerMast = 39, HvacChiller = 40, RooftopPigeonCote = 41,
        // ShopFront (10)
        DudeColaFridge = 42, ChaoMartStand = 43, MenuChalkboard = 44,LotteryTable = 45, FruitCrateRack = 46,
        MotorbikeRepairSign = 47, BarberPole = 48, AtmKiosk = 49, FuelPumpBaCay = 50, UmbrellaParasol = 51,
        // Nature (10)
        TamarindStreetTree = 52, BougainvilleaPlanter = 53, PottedArecaPalm = 54, BananaTreeCluster = 55, RoadsideBush = 56,
        RicePaddyBund = 57, HayBale = 58, CoconutPalm = 59, WeedTuft = 60, LotusPot = 61,
        // GroundDecal (8)
        ManholeCover = 62, StormDrainGrate = 63, CrosswalkZebra = 64, PotholePatch = 65, OilStain = 66,
        SkidMark = 67, WetPuddle = 68, FallenLeaves = 69,
        // NightSign (8)
        NeonNhau247 = 70, NeonKhachHongPhat = 71, NeonKaraoke = 72, LedPharmacyCross = 73, LanternString = 74,
        BillboardLit = 75, TaxiStandBeacon = 76, OpenSignBox = 77
    }

    public struct PropSpec
    {
        public PropKind kind;
        public PropGroup group;
        public SurfaceMaterialType surface;
        public Vector2 footprint;
        public float height;
        public PropFlags flags;
    }

    public static class PropRules
    {
        public const float ChunkSizeMeters = 40f;
        public const int ChunkPropCap = 46;
        public const int PropKindCount = 78;
        public const int GroupCount = 8;

        private static readonly PropSpec[] Catalogue = BuildCatalogue();

        public static int TotalPropKinds => Catalogue.Length;

        public static PropSpec GetSpec(PropKind kind) => Catalogue[(int)kind];

        // §2.9 per-group density: furniture 4, clutter 6, wall detail 7, roof 3, shop front 3, nature 5, ground decal 5, night sign 2.
        public static int BaseDensityForGroup(PropGroup group)
        {
            switch (group)
            {
                case PropGroup.StreetFurniture: return 4;
                case PropGroup.StreetClutter: return 6;
                case PropGroup.WallDetail: return 7;
                case PropGroup.RoofAndUtility: return 3;
                case PropGroup.ShopFront: return 3;
                case PropGroup.Nature: return 5;
                case PropGroup.GroundDecal: return 5;
                case PropGroup.NightSign: return 2;
                default: return 2;
            }
        }

        public static int ComputeChunkPropCount(float districtDensityMultiplier)
        {
            int raw = 0;
            for (int g = 0; g < GroupCount; g++)
                raw += BaseDensityForGroup((PropGroup)g);
            int scaled = Mathf.RoundToInt(raw * Mathf.Max(0f, districtDensityMultiplier));
            return Mathf.Clamp(scaled, 0, ChunkPropCap);
        }

        // §1.7: <= 40 m full + shadows; 40-85 m simplified no shadows; 85-130 m decals only; > 130 m hidden.
        public static PropRenderTier RenderTier(float distanceMeters)
        {
            if (distanceMeters <= 40f) return PropRenderTier.FullShadowed;
            if (distanceMeters <= 85f) return PropRenderTier.SimplifiedNoShadow;
            if (distanceMeters <= 130f) return PropRenderTier.FlatDecal;
            return PropRenderTier.Hidden;
        }

        // <= 8 material groups per district (§1.7 & §1.9.7).
        public static int BatchDrawCalls(bool isNight)
        {
            return isNight ? GroupCount : GroupCount - 1;
        }

        private static PropSpec[] BuildCatalogue()
        {
            PropSpec[] arr = new PropSpec[PropKindCount];
            for (int i = 0; i < PropKindCount; i++)
            {
                PropKind k = (PropKind)i;
                PropGroup grp;
                SurfaceMaterialType surf = SurfaceMaterialType.Metal;
                Vector2 fp = new Vector2(0.8f, 0.8f);
                float h = 1.2f;
                PropFlags flags = PropFlags.StayOffRoad;

                if (i <= 9)
                {
                    grp = PropGroup.StreetFurniture;
                    h = i <= 1 ? 6.5f : 1.4f;
                    flags |= PropFlags.BlocksNavigation;
                    if (k == PropKind.StreetLamp || k == PropKind.TrafficLight) flags |= PropFlags.EmitsLight;
                    if (k == PropKind.FireHydrant || k == PropKind.MailBox) flags |= PropFlags.Breakable;
                }
                else if (i <= 21)
                {
                    grp = PropGroup.StreetClutter;
                    surf = SurfaceMaterialType.Wood;
                    fp = new Vector2(0.5f, 0.5f);
                    h = 0.7f;
                    flags |= PropFlags.Breakable | PropFlags.Grabbable;
                }
                else if (i <= 31)
                {
                    grp = PropGroup.WallDetail;
                    fp = new Vector2(0.9f, 0.35f);
                    h = 0.8f;
                }
                else if (i <= 41)
                {
                    grp = PropGroup.RoofAndUtility;
                    fp = new Vector2(1.4f, 1.4f);
                    h = 2.4f;
                    flags |= PropFlags.BlocksNavigation;
                }
                else if (i <= 51)
                {
                    grp = PropGroup.ShopFront;
                    surf = SurfaceMaterialType.Metal;
                    fp = new Vector2(1.1f, 0.9f);
                    h = 1.8f;
                    flags |= PropFlags.BlocksNavigation | PropFlags.Breakable;
                }
                else if (i <= 61)
                {
                    grp = PropGroup.Nature;
                    surf = SurfaceMaterialType.Wood;
                    fp = new Vector2(1.5f, 1.5f);
                    h = i == 52 || i == 59 ? 7.0f : 1.5f;
                    if (h > 2.5f) flags |= PropFlags.BlocksNavigation;
                }
                else if (i <= 69)
                {
                    grp = PropGroup.GroundDecal;
                    surf = SurfaceMaterialType.Asphalt;
                    fp = new Vector2(2.0f, 2.0f);
                    h = 0.02f;
                    flags = PropFlags.FlatDecal;
                }
                else
                {
                    grp = PropGroup.NightSign;
                    surf = SurfaceMaterialType.Glass;
                    fp = new Vector2(1.6f, 0.3f);
                    h = 1.1f;
                    flags = PropFlags.StayOffRoad | PropFlags.NightOnly | PropFlags.EmitsLight;
                }

                arr[i] = new PropSpec
                {
                    kind = k,
                    group = grp,
                    surface = surf,
                    footprint = fp,
                    height = h,
                    flags = flags
                };
            }
            return arr;
        }
    }
}
