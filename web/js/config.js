window.RHOS_CONFIG = {
  supabaseUrl: "https://hoczacluoagcncwohglw.supabase.co",
  supabaseAnonKey: "",
  downloadUrl: "https://github.com/rhos-42/RhosTool/releases/download/v1.0.0/RhosTool.zip",
  downloadLabel: "RhosTool Portable (Windows)",
  installUrl: "https://github.com/rhos-42/RhosTool/releases/download/v1.0.0/RhosTool-Installer.zip",
  installLabel: "RhosTool Installateur (Windows)",
  downloadVersion: "v1.0.0",
  githubUrl: "https://github.com/rhos-42/RhosTool",
  releasesUrl: "https://github.com/rhos-42/RhosTool/releases/tag/v1.0.0",

  // Hub MediaFire (dossier parent RhosTool)
  mediafireHub: "https://www.mediafire.com/folder/b28cfvp8kjgwl/RhosTool",

  // Lieux MediaFire par catégorie (sous-dossiers)
  resourceFolderUrls: {
    "all-driver": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "ramdisk-a12": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "ramdisk-iphone": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "ramdisk-ipad": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "tools": "https://www.mediafire.com/folder/9z3c0klwqfw71/Tool_Android-PC",
    "packs": "https://www.mediafire.com/folder/k7o5g32nyytdr/Packs_Diag"
  },

  // Chaque ressource pointe vers son lieu MediaFire (dossier catégorie).
  // Remplace par un lien /file/... quand le ZIP est uploadé dans ce dossier.
  resourceUrls: {
    "drv-all-mobile": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-cert-fix": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-apple-usb": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-huawei-kirin": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-huawei-usb10": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-libusb-filter": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-nokia-mtk": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-nokia-full": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-qc-mtk": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-qc-mtk-mf": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-spd-77xx": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-spd-77xx-mf": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-sprd-npi": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-installer-pack": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-installer-alt": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-fix-mtk-com": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-fix-mtk-com-mf": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-fix-qc": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-winra1n": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",
    "drv-vcredist-2015": "https://www.mediafire.com/folder/wlbrfxg0zdztx/All_Driver",

    "rd-a12-iphone-xr": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-xs": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-xs-max": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-11": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-11-pro": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-11-pro-max": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-iphone-se-2020": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-ipad-8": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-ipad-9": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-ipad-air-3": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",
    "rd-a12-ipad-mini-5": "https://www.mediafire.com/folder/x5r397fbnv65r/Ramdisk_A12-A13",

    "rd-iphone-5se": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-6g": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-6-plus": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-6s": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-6s-plus": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-7g": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-7-plus": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-8g": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-8-plus": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",
    "rd-iphone-x": "https://www.mediafire.com/folder/167i63pk2kg37/Ramdisk_iPhone",

    "rd-ipad-5": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-6": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-7": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-air-2": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-mini-4": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-pro-105": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-pro-129-g1": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-pro-129-g2": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",
    "rd-ipad-pro-97": "https://www.mediafire.com/folder/i50xlceciezlh/Ramdisk_iPad",

    "tool-pc": "https://www.mediafire.com/folder/9z3c0klwqfw71/Tool_Android-PC",
    "tool-winra1n": "https://www.mediafire.com/folder/9z3c0klwqfw71/Tool_Android-PC",

    "res-diag-iphone-ipad": "https://www.mediafire.com/folder/k7o5g32nyytdr/Packs_Diag",
    "res-all-ramdisk-iphone": "https://www.mediafire.com/folder/k7o5g32nyytdr/Packs_Diag",
    "res-all-ramdisk-ipad": "https://www.mediafire.com/folder/k7o5g32nyytdr/Packs_Diag"
  }
};
