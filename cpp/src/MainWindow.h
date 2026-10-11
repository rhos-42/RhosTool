#pragma once

#include "AdbClient.h"
#include "CatalogStore.h"

#include <QPoint>
#include <QVector>
#include <QWidget>

class QLabel;
class QLineEdit;
class QListWidget;
class QPlainTextEdit;
class QProgressBar;
class QPushButton;
class QGridLayout;
class QMouseEvent;
class QComboBox;

class MainWindow : public QWidget {
    Q_OBJECT
public:
    explicit MainWindow(QWidget* parent = nullptr);

protected:
    void mousePressEvent(QMouseEvent* e) override;
    void mouseMoveEvent(QMouseEvent* e) override;
    void mouseReleaseEvent(QMouseEvent* e) override;
    void mouseDoubleClickEvent(QMouseEvent* e) override;

private:
    void buildUi();
    void applyStyle();
    QWidget* buildTitleBar();
    QWidget* buildBrandGrid();
    QWidget* buildLeftPanel();
    QWidget* buildCenterPanel();
    QWidget* buildRightPanel();
    QWidget* buildStatusBar();

    void selectBrand(int index);
    void selectFunctionTab(int index);
    void consolidateTabs();
    void rebuildModeTabs();
    void rebuildFuncTabs();
    void rebuildModels();
    void rebuildOps();
    void filterModels(const QString& text);
    void onModelSelected();
    const CatalogModel* currentModel() const;
    bool isResourceTab(const QString& title) const;
    bool tabVisible(const CatalogTab& tab, const CatalogBrand& brand, const CatalogModel* model) const;
    void openConfigDialog();
    void runCatalogOp(const CatalogOp& op);
    void scanDevices();
    void logLine(const QString& line);
    void openUrl(const QString& url);
    void updateClock();
    void toggleMaxRestore();
    QString askText(const QString& title, const QString& label, const QString& def = {});

    AdbClient adb_;
    QVector<CatalogBrand> brands_;
    QVector<CatalogModel> brandModels_;
    int brandIndex_ = 0;
    int funcTabIndex_ = 0;
    QString currentSerial_;
    bool dragging_ = false;
    QPoint dragPos_;

    QWidget* titleBar_ = nullptr;
    QGridLayout* brandGrid_ = nullptr;
    QList<QPushButton*> brandButtons_;
    QWidget* modeTabsHost_ = nullptr;
    QLineEdit* search_ = nullptr;
    QListWidget* models_ = nullptr;
    QLabel* modelCount_ = nullptr;
    QWidget* funcTabsBar_ = nullptr;
    QList<QPushButton*> funcTabButtons_;
    QLabel* modelLabel_ = nullptr;
    QComboBox* chipCombo_ = nullptr;
    QWidget* opsHost_ = nullptr;
    QPlainTextEdit* console_ = nullptr;
    QLabel* usbLabel_ = nullptr;
    QLabel* statusLeft_ = nullptr;
    QLabel* statusCredits_ = nullptr;
    QLabel* statusUser_ = nullptr;
    QLabel* statusClock_ = nullptr;
    QLabel* catalogStats_ = nullptr;
    QProgressBar* progress_ = nullptr;
    QPushButton* maxBtn_ = nullptr;
};
