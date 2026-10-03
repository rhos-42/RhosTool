using RhosTool.Core.Models;

namespace RhosTool.Core.Services;

public static partial class ModelCatalog
{
    private static void AddHuawei(List<PhoneModel> list)
    {
        AddNamed(list, "huawei", "Huawei P",
            ("Huawei P40 Pro", "ELS-NX9", "Kirin 990 5G"),
            ("Huawei P50", "JAD-LX9", "Snapdragon 888 4G"),
            ("Huawei P50 Pro", "JAD-AL50", "Snapdragon 888 4G"),
            ("Huawei P60", "MNA-LX9", "Snapdragon 8+ Gen 1 4G"),
            ("Huawei P60 Pro", "MNA-AL00", "Snapdragon 8+ Gen 1 4G"),
            ("Huawei P70", "PLB-AL00", "Kirin 9010"),
            ("Huawei P70 Pro", "PLB-AL10", "Kirin 9010"),
            ("Huawei Pura 70", "ADY-AL00", "Kirin 9010"),
            ("Huawei Pura 70 Pro", "ADY-AL10", "Kirin 9010"),
            ("Huawei Pura 70 Ultra", "ADY-AL20", "Kirin 9010"),
            ("Huawei Pura 80", "PLD-AL00", "Kirin 9020"),
            ("Huawei Pura 80 Pro", "PLD-AL10", "Kirin 9020"),
            ("Huawei Pura 80 Ultra", "PLD-AL20", "Kirin 9020"));

        AddNamed(list, "huawei", "Huawei Mate",
            ("Huawei Mate 50 Pro", "DCO-AL00", "Snapdragon 8+ Gen 1 4G"),
            ("Huawei Mate 60", "BRA-AL00", "Kirin 9000S"),
            ("Huawei Mate 60 Pro", "ALN-AL00", "Kirin 9000S"),
            ("Huawei Mate 60 Pro+", "ALN-AL10", "Kirin 9000S"),
            ("Huawei Mate 70", "BRT-AL00", "Kirin 9020"),
            ("Huawei Mate 70 Pro", "BRT-AL10", "Kirin 9020"),
            ("Huawei Mate 70 Pro+", "BRT-AL20", "Kirin 9020"),
            ("Huawei Mate 70 RS", "BRT-AL80", "Kirin 9020"),
            ("Huawei Mate X3", "ALT-AL10", "Snapdragon 8+ Gen 1 4G"),
            ("Huawei Mate X5", "PAL-AL00", "Kirin 9000S"),
            ("Huawei Mate X6", "TET-AL00", "Kirin 9020"),
            ("Huawei Mate XT", "GRL-AL10", "Kirin 9010"));

        AddNamed(list, "huawei", "Huawei Nova",
            ("Huawei Nova 11", "GOA-LX9", "Snapdragon 778G"),
            ("Huawei Nova 12", "ADA-AL00", "Snapdragon 7 Gen 1"),
            ("Huawei Nova 12 Ultra", "ADA-AL10", "Snapdragon 8+ Gen 1 4G"),
            ("Huawei Nova 13", "BLK-AL00", "Snapdragon 7s Gen 2"),
            ("Huawei Nova 13 Pro", "BLK-AL10", "Snapdragon 7+ Gen 2"),
            ("Huawei Nova 14", "CLD-AL00", "Snapdragon 7s Gen 3"),
            ("Huawei Nova 14 Pro", "CLD-AL10", "Snapdragon 8s Gen 3"));

        AddNamed(list, "huawei", "Huawei Enjoy / Y",
            ("Huawei Enjoy 70", "STG-AL00", "Snapdragon 680"),
            ("Huawei Enjoy 70 Pro", "STG-AL10", "Snapdragon 6 Gen 1"),
            ("Huawei Enjoy 80", "STH-AL00", "Snapdragon 6s Gen 3"),
            ("Huawei Y9a", "MLA-LX9", "Helio G80"),
            ("Huawei Y9s", "STK-L21", "Kirin 710F"));
    }

    private static void AddHonor(List<PhoneModel> list)
    {
        AddNamed(list, "honor", "Honor Number",
            ("Honor 90", "REA-NX9", "Snapdragon 7 Gen 1"),
            ("Honor 90 Lite", "CRT-NX1", "Dimensity 6020"),
            ("Honor 200", "ELI-NX9", "Snapdragon 7 Gen 3"),
            ("Honor 200 Lite", "ELI-NX1", "Dimensity 7025"),
            ("Honor 200 Pro", "ELP-NX9", "Snapdragon 8s Gen 3"),
            ("Honor 400", "FCP-NX1", "Snapdragon 7 Gen 3"),
            ("Honor 400 Lite", "FCP-LX1", "Dimensity 7025 Ultra"),
            ("Honor 400 Pro", "FCP-NX9", "Snapdragon 8s Gen 3"),
            ("Honor 500", "GCP-NX1", "Snapdragon 7s Gen 3"),
            ("Honor 500 Pro", "GCP-NX9", "Snapdragon 8s Gen 4"));

        AddNamed(list, "honor", "Honor X",
            ("Honor X6", "VNE-LX1", "Helio G25"),
            ("Honor X6b", "VNE-LX3", "Helio G85"),
            ("Honor X7", "CMA-LX1", "Snapdragon 680"),
            ("Honor X7b", "CMA-LX3", "Snapdragon 680"),
            ("Honor X7c", "CMA-LX5", "Snapdragon 4 Gen 2"),
            ("Honor X8", "TFY-LX1", "Snapdragon 680"),
            ("Honor X8b", "RKY-LX1", "Snapdragon 680"),
            ("Honor X8c", "RKY-LX3", "Snapdragon 4 Gen 2"),
            ("Honor X9a", "RMO-NX1", "Snapdragon 695"),
            ("Honor X9b", "ANY-NX1", "Snapdragon 6 Gen 1"),
            ("Honor X9c", "ANY-NX3", "Snapdragon 6 Gen 1"),
            ("Honor X9c Smart", "ANY-NX5", "Snapdragon 6 Gen 1"));

        AddNamed(list, "honor", "Honor Magic",
            ("Honor Magic5 Pro", "PGT-N09", "Snapdragon 8 Gen 2"),
            ("Honor Magic6", "BVL-N49", "Snapdragon 8 Gen 3"),
            ("Honor Magic6 Pro", "BVL-N19", "Snapdragon 8 Gen 3"),
            ("Honor Magic6 Lite", "BVL-NX1", "Snapdragon 6 Gen 1"),
            ("Honor Magic7", "PTP-N49", "Snapdragon 8 Elite"),
            ("Honor Magic7 Pro", "PTP-N19", "Snapdragon 8 Elite"),
            ("Honor Magic7 Lite", "PTP-NX1", "Snapdragon 7 Gen 3"),
            ("Honor Magic8", "QTP-N49", "Snapdragon 8 Elite Gen 2"),
            ("Honor Magic8 Pro", "QTP-N19", "Snapdragon 8 Elite Gen 2"),
            ("Honor Magic V2", "VER-N49", "Snapdragon 8 Gen 2"),
            ("Honor Magic V3", "FCP-N49", "Snapdragon 8 Gen 3"),
            ("Honor Magic V5", "GTP-N49", "Snapdragon 8 Elite"),
            ("Honor Magic Vs3", "FRI-N49", "Snapdragon 8 Gen 2"));
    }

    private static void AddNamed(
        List<PhoneModel> list,
        string brandId,
        string series,
        params (string Name, string Code, string Chip)[] models)
    {
        foreach (var (name, code, chip) in models)
            list.Add(M(brandId, series, name, code, chip));
    }
}
