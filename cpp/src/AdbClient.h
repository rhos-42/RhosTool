#pragma once

#include <QList>
#include <QString>
#include <QStringList>

struct AdbDevice {
    QString serial;
    QString state;
    bool authorized = false;
};

class AdbClient {
public:
    QString adbPath() const;
    bool available() const;
    QString run(const QStringList& args, int timeoutMs = 30000) const;
    QString runOn(const QString& serial, const QStringList& args, int timeoutMs = 30000) const;
    QList<AdbDevice> devices() const;
    QString readInfo(const QString& serial) const;
    QString shell(const QString& serial, const QString& cmd, int timeoutMs = 30000) const;

private:
    mutable QString cachedPath_;
    QString resolve() const;
};
