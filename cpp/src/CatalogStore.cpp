#include "CatalogStore.h"

#include <QCoreApplication>
#include <QDir>
#include <QFile>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>

CatalogStore& CatalogStore::instance() {
    static CatalogStore s;
    return s;
}

static QString findDataDir() {
    const QStringList candidates = {
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("data")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../data")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../../data")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../../../cpp/data")),
    };
    for (const auto& c : candidates) {
        if (QFile::exists(c + QStringLiteral("/brands.json")) &&
            QFile::exists(c + QStringLiteral("/models.json")))
            return QDir(c).absolutePath();
    }
    return {};
}

bool CatalogStore::load(const QString& dataDirIn) {
    const QString dataDir = dataDirIn.isEmpty() ? findDataDir() : dataDirIn;
    if (dataDir.isEmpty()) return false;

    auto readJson = [&](const QString& name) -> QJsonDocument {
        QFile f(QDir(dataDir).filePath(name));
        if (!f.open(QIODevice::ReadOnly)) return {};
        return QJsonDocument::fromJson(f.readAll());
    };

    const auto brandsDoc = readJson(QStringLiteral("brands.json"));
    const auto modelsDoc = readJson(QStringLiteral("models.json"));
    const auto mfDoc = readJson(QStringLiteral("mediafire.json"));
    if (!brandsDoc.isArray() || !modelsDoc.isArray()) return false;

    brands_.clear();
    for (const auto& v : brandsDoc.array()) {
        const auto o = v.toObject();
        CatalogBrand b;
        b.id = o.value(QStringLiteral("Id")).toString();
        if (b.id.isEmpty()) b.id = o.value(QStringLiteral("id")).toString();
        b.name = o.value(QStringLiteral("Name")).toString();
        if (b.name.isEmpty()) b.name = o.value(QStringLiteral("name")).toString();
        b.shortName = o.value(QStringLiteral("ShortName")).toString();
        if (b.shortName.isEmpty()) b.shortName = o.value(QStringLiteral("shortName")).toString();
        b.accentHex = o.value(QStringLiteral("AccentHex")).toString();
        if (b.accentHex.isEmpty()) b.accentHex = o.value(QStringLiteral("accentHex")).toString();
        b.platform = o.value(QStringLiteral("Platform")).toString();
        if (b.platform.isEmpty()) b.platform = o.value(QStringLiteral("platform")).toString();
        for (const auto& m : o.value(QStringLiteral("modes")).toArray())
            b.modes.push_back(m.toString());
        for (const auto& tv : o.value(QStringLiteral("tabs")).toArray()) {
            const auto to = tv.toObject();
            CatalogTab t;
            t.id = to.value(QStringLiteral("Id")).toString();
            if (t.id.isEmpty()) t.id = to.value(QStringLiteral("id")).toString();
            t.title = to.value(QStringLiteral("Title")).toString();
            if (t.title.isEmpty()) t.title = to.value(QStringLiteral("title")).toString();
            for (const auto& ov : to.value(QStringLiteral("ops")).toArray()) {
                const auto oo = ov.toObject();
                CatalogOp op;
                op.id = oo.value(QStringLiteral("Id")).toString();
                if (op.id.isEmpty()) op.id = oo.value(QStringLiteral("id")).toString();
                op.title = oo.value(QStringLiteral("Title")).toString();
                if (op.title.isEmpty()) op.title = oo.value(QStringLiteral("title")).toString();
                op.description = oo.value(QStringLiteral("Description")).toString();
                if (op.description.isEmpty()) op.description = oo.value(QStringLiteral("description")).toString();
                op.kind = oo.value(QStringLiteral("kind")).toString();
                op.requiresDevice = oo.value(QStringLiteral("RequiresDevice")).toBool(true);
                if (oo.contains(QStringLiteral("requiresDevice")))
                    op.requiresDevice = oo.value(QStringLiteral("requiresDevice")).toBool(true);
                op.requiredMode = oo.value(QStringLiteral("requiredMode")).toString();
                t.ops.push_back(op);
            }
            b.tabs.push_back(t);
        }
        brands_.push_back(b);
    }

    modelsByBrand_.clear();
    for (const auto& v : modelsDoc.array()) {
        const auto o = v.toObject();
        CatalogModel m;
        m.brandId = o.value(QStringLiteral("brandId")).toString();
        m.name = o.value(QStringLiteral("name")).toString();
        m.code = o.value(QStringLiteral("code")).toString();
        m.chip = o.value(QStringLiteral("chip")).toString();
        m.series = o.value(QStringLiteral("series")).toString();
        for (const auto& c : o.value(QStringLiteral("codes")).toArray())
            m.codes.push_back(c.toString());
        for (const auto& t : o.value(QStringLiteral("tags")).toArray())
            m.tags.push_back(t.toString());
        modelsByBrand_[m.brandId.toLower()].push_back(m);
    }

    mediafire_.clear();
    if (mfDoc.isObject()) {
        const auto o = mfDoc.object();
        for (auto it = o.begin(); it != o.end(); ++it)
            mediafire_[it.key()] = it.value().toString();
    }

    loaded_ = !brands_.isEmpty();
    return loaded_;
}

QVector<CatalogModel> CatalogStore::modelsForBrand(const QString& brandId) const {
    return modelsByBrand_.value(brandId.toLower());
}

QString CatalogStore::mediafireUrl(const QString& key) const {
    return mediafire_.value(key);
}

QString CatalogStore::urlForOp(const QString& opId) const {
    if (opId.startsWith(QStringLiteral("drv-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("all-driver"));
    if (opId.startsWith(QStringLiteral("rd-a12-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("ramdisk-a12"));
    if (opId.startsWith(QStringLiteral("rd-iphone-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("ramdisk-iphone"));
    if (opId.startsWith(QStringLiteral("rd-ipad-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("ramdisk-ipad"));
    if (opId.startsWith(QStringLiteral("tool-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("tools"));
    if (opId.startsWith(QStringLiteral("res-"), Qt::CaseInsensitive))
        return mediafireUrl(QStringLiteral("packs"));
    return {};
}
