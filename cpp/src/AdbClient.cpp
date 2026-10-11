#include "AdbClient.h"

#include <QCoreApplication>
#include <QDir>
#include <QFileInfo>
#include <QProcess>
#include <QStandardPaths>

QString AdbClient::resolve() const {
    if (!cachedPath_.isEmpty() && QFileInfo::exists(cachedPath_))
        return cachedPath_;

    const QString appData = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    const QStringList candidates = {
        QDir(appData).filePath(QStringLiteral("platform-tools/adb.exe")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("platform-tools/adb.exe")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../tools/platform-tools/adb.exe")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../../tools/platform-tools/adb.exe")),
        QDir(QCoreApplication::applicationDirPath()).filePath(QStringLiteral("../../../tools/platform-tools/adb.exe")),
    };
    for (const auto& path : candidates) {
        if (QFileInfo::exists(path)) {
            cachedPath_ = QFileInfo(path).absoluteFilePath();
            return cachedPath_;
        }
    }

    QProcess which;
    which.start(QStringLiteral("where"), {QStringLiteral("adb.exe")});
    if (which.waitForFinished(3000) && which.exitCode() == 0) {
        const auto line = QString::fromLocal8Bit(which.readAllStandardOutput()).split('\n', Qt::SkipEmptyParts);
        if (!line.isEmpty()) {
            cachedPath_ = line.first().trimmed();
            return cachedPath_;
        }
    }
    return {};
}

QString AdbClient::adbPath() const { return resolve(); }
bool AdbClient::available() const { return QFileInfo::exists(resolve()); }

QString AdbClient::run(const QStringList& args, int timeoutMs) const {
    const auto exe = resolve();
    if (exe.isEmpty())
        return QStringLiteral("ADB introuvable (tools/platform-tools/adb.exe).");

    QProcess proc;
    proc.setProcessChannelMode(QProcess::MergedChannels);
    proc.start(exe, args);
    if (!proc.waitForFinished(timeoutMs)) {
        proc.kill();
        return QStringLiteral("ADB timeout.");
    }
    return QString::fromLocal8Bit(proc.readAll()).trimmed();
}

QString AdbClient::runOn(const QString& serial, const QStringList& args, int timeoutMs) const {
    QStringList full{QStringLiteral("-s"), serial};
    full.append(args);
    return run(full, timeoutMs);
}

QList<AdbDevice> AdbClient::devices() const {
    QList<AdbDevice> list;
    if (!available()) return list;
    run({QStringLiteral("start-server")}, 8000);
    const auto text = run({QStringLiteral("devices"), QStringLiteral("-l")});
    for (const auto& raw : text.split('\n', Qt::SkipEmptyParts)) {
        if (raw.startsWith(QStringLiteral("List of devices"), Qt::CaseInsensitive))
            continue;
        const auto parts = raw.simplified().split(' ');
        if (parts.size() < 2) continue;
        AdbDevice d;
        d.serial = parts[0];
        d.state = parts[1];
        d.authorized = d.state.compare(QStringLiteral("device"), Qt::CaseInsensitive) == 0;
        list.push_back(d);
    }
    return list;
}

QString AdbClient::shell(const QString& serial, const QString& cmd, int timeoutMs) const {
    return runOn(serial, {QStringLiteral("shell"), cmd}, timeoutMs);
}

QString AdbClient::readInfo(const QString& serial) const {
    const QStringList keys = {
        QStringLiteral("ro.product.manufacturer"), QStringLiteral("ro.product.model"),
        QStringLiteral("ro.product.device"), QStringLiteral("ro.build.version.release"),
        QStringLiteral("ro.build.version.sdk"), QStringLiteral("ro.serialno"),
        QStringLiteral("ro.product.cpu.abi"), QStringLiteral("gsm.version.baseband")
    };
    QString report;
    for (const auto& key : keys)
        report += key + QStringLiteral(" = ") + shell(serial, QStringLiteral("getprop ") + key, 8000) + '\n';
    return report.trimmed();
}
