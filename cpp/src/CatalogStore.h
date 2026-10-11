#pragma once

#include <QHash>
#include <QString>
#include <QStringList>
#include <QVector>

struct CatalogOp {
    QString id;
    QString title;
    QString description;
    QString kind;
    bool requiresDevice = true;
    QString requiredMode;
};

struct CatalogTab {
    QString id;
    QString title;
    QVector<CatalogOp> ops;
};

struct CatalogBrand {
    QString id;
    QString name;
    QString shortName;
    QString accentHex;
    QString platform;
    QStringList modes;
    QVector<CatalogTab> tabs;
};

struct CatalogModel {
    QString brandId;
    QString name;
    QString code;
    QStringList codes;
    QString chip;
    QString series;
    QStringList tags;

    QString codesDisplay() const {
        if (!codes.isEmpty()) return codes.join(QStringLiteral(" / "));
        return code;
    }
};

class CatalogStore {
public:
    static CatalogStore& instance();
    bool load(const QString& dataDir);
    bool loaded() const { return loaded_; }
    const QVector<CatalogBrand>& brands() const { return brands_; }
    QVector<CatalogModel> modelsForBrand(const QString& brandId) const;
    QString mediafireUrl(const QString& key) const;
    QString urlForOp(const QString& opId) const;

private:
    CatalogStore() = default;
    bool loaded_ = false;
    QVector<CatalogBrand> brands_;
    QHash<QString, QVector<CatalogModel>> modelsByBrand_;
    QHash<QString, QString> mediafire_;
};
