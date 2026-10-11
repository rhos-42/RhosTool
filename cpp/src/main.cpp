#include "MainWindow.h"

#include <QApplication>
#include <QDir>
#include <QLockFile>
#include <QMessageBox>
#include <QScreen>
#include <QStandardPaths>

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);
    QApplication::setApplicationName(QStringLiteral("RhosTool"));
    QApplication::setOrganizationName(QStringLiteral("RhosTool"));

    const auto lockDir = QStandardPaths::writableLocation(QStandardPaths::AppDataLocation);
    QDir().mkpath(lockDir);
    QLockFile lock(lockDir + QStringLiteral("/rhostool.lock"));
    lock.setStaleLockTime(0);
    if (!lock.tryLock(100)) {
        QMessageBox::information(nullptr, QStringLiteral("RhosTool"),
                                 QStringLiteral("RhosTool est déjà en cours d'exécution."));
        return 0;
    }

    MainWindow window;
    window.show();
    if (auto* screen = QApplication::primaryScreen()) {
        const QRect geo = screen->availableGeometry();
        const int w = qMin(1280, geo.width() - 40);
        const int h = qMin(840, geo.height() - 40);
        window.resize(w, h);
        const int x = geo.x() + (geo.width() - w) / 2;
        const int y = qMax(geo.y(), geo.y() + (geo.height() - h) / 2);
        window.move(x, y);
    }
    window.raise();
    window.activateWindow();
    return app.exec();
}
