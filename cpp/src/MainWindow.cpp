#include "MainWindow.h"
#include "FlowLayout.h"

#include <QApplication>
#include <QCheckBox>
#include <QColor>
#include <QComboBox>
#include <QDateTime>
#include <QDesktopServices>
#include <QDialog>
#include <QFileDialog>
#include <QFrame>
#include <QGridLayout>
#include <QIcon>
#include <QPainter>
#include <QPixmap>
#include <QHBoxLayout>
#include <QInputDialog>
#include <QLabel>
#include <QLineEdit>
#include <QListWidget>
#include <QMouseEvent>
#include <QPlainTextEdit>
#include <QProcess>
#include <QProgressBar>
#include <QPushButton>
#include <QScrollArea>
#include <QSplitter>
#include <QTimer>
#include <QUrl>
#include <QVBoxLayout>

MainWindow::MainWindow(QWidget* parent) : QWidget(parent) {
    setWindowTitle(QStringLiteral("RhosTool"));
    setWindowFlags(Qt::Window | Qt::FramelessWindowHint);
    setMinimumSize(720, 480);
    resize(1280, 820);
    applyStyle();
    buildUi();

    auto& cat = CatalogStore::instance();
    if (!cat.load({})) {
        logLine(QStringLiteral("ERREUR : catalogues JSON introuvables (cpp/data)."));
    } else {
        brands_ = cat.brands();
        consolidateTabs();
        // Rebuild brand grid from catalog
        if (brandGrid_) {
            QLayoutItem* child;
            while ((child = brandGrid_->takeAt(0)) != nullptr) {
                delete child->widget();
                delete child;
            }
            brandButtons_.clear();
            const int rows = 3;
            const int cols = qMax(1, (brands_.size() + rows - 1) / rows);
            for (int i = 0; i < brands_.size(); ++i) {
                const auto& b = brands_[i];
                auto* btn = new QPushButton(b.shortName, brandGrid_->parentWidget());
                btn->setObjectName(QStringLiteral("BrandTile"));
                btn->setCheckable(true);
                btn->setCursor(Qt::PointingHandCursor);
                btn->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Expanding);
                btn->setMinimumHeight(34);
                QColor base(b.accentHex);
                if (!base.isValid()) base = QColor(QStringLiteral("#555555"));
                const QString light = base.lighter(150).name();
                const QString mid   = base.name();
                const QString dark  = base.darker(155).name();
                btn->setStyleSheet(QStringLiteral(
                    "QPushButton#BrandTile {"
                    "  background: qlineargradient(x1:0,y1:0,x2:0.4,y2:1,"
                    "    stop:0 %1, stop:0.5 %2, stop:1 %3);"
                    "  color:white; font-weight:800; font-size:12px; letter-spacing:0.3px;"
                    "  border:1px solid rgba(0,0,0,0.5); border-top:1px solid rgba(255,255,255,0.32);"
                    "  border-radius:7px; padding:2px;"
                    "}"
                    "QPushButton#BrandTile:hover { border:1px solid #FFB74D; }"
                    "QPushButton#BrandTile:checked { border:2px solid #F57C00; }")
                    .arg(light, mid, dark));
                const int idx = i;
                connect(btn, &QPushButton::clicked, this, [this, idx] { selectBrand(idx); });
                brandGrid_->addWidget(btn, i / cols, i % cols);
                brandButtons_.push_back(btn);
            }
            for (int c = 0; c < cols; ++c) brandGrid_->setColumnStretch(c, 1);
            for (int r = 0; r < rows; ++r) brandGrid_->setRowStretch(r, 1);
        }
        int totalModels = 0;
        for (const auto& b : brands_)
            totalModels += cat.modelsForBrand(b.id).size();
        catalogStats_->setText(QStringLiteral("%1 marques · %2 modèles").arg(brands_.size()).arg(totalModels));
        logLine(QStringLiteral("Catalogues chargés : %1 marques, %2 modèles.")
                    .arg(brands_.size()).arg(totalModels));
        selectBrand(1); // Samsung
    }

    scanDevices();
    auto* clock = new QTimer(this);
    connect(clock, &QTimer::timeout, this, &MainWindow::updateClock);
    clock->start(1000);
    updateClock();
}

void MainWindow::applyStyle() {
    setObjectName(QStringLiteral("Root"));
    setStyleSheet(QStringLiteral(R"(
        QWidget#Root {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #262626, stop:1 #171717);
            color: #ECECEC; border: 1px solid #0A0A0A; font-family: "Segoe UI"; font-size: 12px;
        }
        QDialog#Root { background: #1C1C1C; color: #ECECEC; font-family: "Segoe UI"; font-size: 12px; }
        QWidget#TitleBar {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #2E2E2E, stop:1 #202020);
            border-bottom: 1px solid #0A0A0A;
        }
        QLabel#TitleText { color: #F0F0F0; font-size: 12px; font-weight: 700; letter-spacing: 0.4px; }
        QPushButton#CaptionBtn, QPushButton#CaptionClose {
            background: transparent; border: none; color: #D8D8D8; min-width: 46px; max-width: 46px; min-height: 36px;
        }
        QPushButton#CaptionBtn:hover { background: rgba(255,255,255,0.10); color: #fff; }
        QPushButton#CaptionClose:hover { background: #E81123; color: #fff; }

        QWidget#BrandStrip {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #1C1C1C, stop:1 #161616);
            border-bottom: 1px solid #000;
        }
        QWidget#LeftPanel, QWidget#RightPanel { background: #1F1F1F; }
        QWidget#CenterPanel { background: #191919; }

        QLineEdit {
            background: #141414; border: 1px solid #3A3A3A; border-radius: 6px; padding: 7px 10px;
            color: #F5F5F5; selection-background-color: #F57C00; selection-color: #111;
        }
        QLineEdit:focus { border: 1px solid #F57C00; }

        QListWidget {
            background: #141414; border: 1px solid #333; border-radius: 6px; color: #E2E2E2; outline: none;
        }
        QListWidget::item { padding: 7px 9px; border-bottom: 1px solid #202020; }
        QListWidget::item:hover { background: #202020; }
        QListWidget::item:selected {
            background: qlineargradient(x1:0,y1:0,x2:1,y2:0, stop:0 #3A2C08, stop:1 #262010);
            color: #FFC048; border-left: 3px solid #F57C00;
        }

        QPushButton#ModeTab {
            background: #2C2C2C; color: #D0D0D0; border: 1px solid #434343; border-radius: 14px;
            padding: 5px 15px; font-weight: 700; font-size: 12px;
        }
        QPushButton#ModeTab:hover { border-color: #FFB74D; color: #fff; }
        QPushButton#ModeTab:checked {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #FFB300, stop:1 #F57C00);
            color: #1A1300; border-color: #F57C00;
        }

        QPushButton#FuncTab {
            background: transparent; color: #B8B8B8; border: none; border-bottom: 2px solid transparent;
            padding: 8px 14px; font-weight: 700; font-size: 12px;
        }
        QPushButton#FuncTab:hover { color: #FFFFFF; }
        QPushButton#FuncTab:checked { color: #FB8C00; border-bottom: 2px solid #F57C00; }

        QPushButton#ActionBtn {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #3A3A3A, stop:1 #2B2B2B);
            color: #F0C24A; border: 1px solid #474747; border-radius: 7px;
            min-width: 128px; min-height: 40px; padding: 8px 12px; font-weight: 700; font-size: 12px;
        }
        QPushButton#ActionBtn:hover { border-color: #F57C00; color: #FFC66B; }
        QPushButton#ActionBtn:pressed { background: #242424; }

        QScrollArea#OpsScroll, QWidget#OpsHost { background: #171717; border: none; }
        QPushButton#OpRow {
            background: #1C1C1C; color: #E6E6E6; border: none;
            border-bottom: 1px solid #262626; border-left: 3px solid transparent;
            text-align: left; padding: 10px 14px; font-size: 12px; font-weight: 600;
        }
        QPushButton#OpRow:hover {
            background: qlineargradient(x1:0,y1:0,x2:1,y2:0, stop:0 #2E2411, stop:0.06 #262626, stop:1 #202020);
            color: #FFFFFF; border-left: 3px solid #F57C00;
        }
        QPushButton#OpRow:pressed { background: #2E2411; }

        QPushButton#UtilBtn {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #2B2B2B, stop:1 #202020);
            color: #FFB74D; border: 1px solid #434343; border-radius: 7px; min-height: 44px;
            font-weight: 800; font-size: 12px; letter-spacing: 0.3px;
        }
        QPushButton#UtilBtn:hover { border-color: #F57C00; color: #FFC66B; }
        QPushButton#UtilBtn:pressed { background: #1C1C1C; }

        QPushButton#StopBtn {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #D32F2F, stop:1 #B71C1C);
            color: white; border: 1px solid #FF5252; border-radius: 7px; min-width: 80px; min-height: 44px; font-weight: 800;
        }
        QPushButton#StopBtn:hover { border-color: #FF8A80; }

        QPlainTextEdit#Console {
            background: #0E0E0E; color: #D4D4D4; border: 1px solid #2E2E2E; border-radius: 6px;
            font-family: "Cascadia Mono", Consolas, monospace; font-size: 13px; padding: 8px;
            selection-background-color: #F57C00; selection-color: #111;
        }

        QProgressBar {
            background: #121212; border: 1px solid #333; border-radius: 7px; text-align: center;
            color: #1A1300; font-weight: 800; min-height: 22px;
        }
        QProgressBar::chunk {
            border-radius: 6px;
            background: qlineargradient(x1:0,y1:0,x2:1,y2:0, stop:0 #FFC107, stop:1 #F57C00);
        }

        QWidget#StatusBar {
            background: qlineargradient(x1:0,y1:0,x2:0,y2:1, stop:0 #242424, stop:1 #1C1C1C);
            border-top: 1px solid #000;
        }
        QLabel#Accent { color: #FB8C00; font-weight: 700; }
        QCheckBox { color: #DADADA; spacing: 6px; }
        QCheckBox::indicator { width: 15px; height: 15px; border: 1px solid #555; border-radius: 3px; background: #141414; }
        QCheckBox::indicator:checked { background: #F57C00; border-color: #F57C00; }
        QComboBox {
            background: #141414; color: #EEE; border: 1px solid #3A3A3A; border-radius: 6px; padding: 5px 10px; min-width: 72px;
        }
        QComboBox:hover { border-color: #F57C00; }
        QComboBox QAbstractItemView { background: #1A1A1A; color: #EEE; border: 1px solid #3A3A3A; selection-background-color: #F57C00; selection-color: #111; }
        QScrollArea { border: none; background: transparent; }

        QScrollBar:vertical { background: transparent; width: 10px; margin: 2px; }
        QScrollBar::handle:vertical { background: #3E3E3E; border-radius: 5px; min-height: 32px; }
        QScrollBar::handle:vertical:hover { background: #F57C00; }
        QScrollBar::add-line:vertical, QScrollBar::sub-line:vertical { height: 0; }
        QScrollBar::add-page:vertical, QScrollBar::sub-page:vertical { background: transparent; }
        QScrollBar:horizontal { background: transparent; height: 10px; margin: 2px; }
        QScrollBar::handle:horizontal { background: #3E3E3E; border-radius: 5px; min-width: 32px; }
        QScrollBar::handle:horizontal:hover { background: #F57C00; }
        QScrollBar::add-line:horizontal, QScrollBar::sub-line:horizontal { width: 0; }
        QScrollBar::add-page:horizontal, QScrollBar::sub-page:horizontal { background: transparent; }

        QSplitter::handle { background: #121212; }
        QSplitter::handle:horizontal { width: 4px; }
        QSplitter::handle:horizontal:hover { background: #F57C00; }
    )"));
}

void MainWindow::buildUi() {
    auto* root = new QVBoxLayout(this);
    root->setContentsMargins(0, 0, 0, 0);
    root->setSpacing(0);
    root->addWidget(buildTitleBar());
    root->addWidget(buildBrandGrid());
    auto* split = new QSplitter(Qt::Horizontal, this);
    split->addWidget(buildLeftPanel());
    split->addWidget(buildCenterPanel());
    split->addWidget(buildRightPanel());
    split->setStretchFactor(0, 22);
    split->setStretchFactor(1, 30);
    split->setStretchFactor(2, 28);
    split->setChildrenCollapsible(false);
    root->addWidget(split, 1);
    root->addWidget(buildStatusBar());
}

QWidget* MainWindow::buildTitleBar() {
    titleBar_ = new QWidget(this);
    titleBar_->setObjectName(QStringLiteral("TitleBar"));
    titleBar_->setFixedHeight(36);
    auto* lay = new QHBoxLayout(titleBar_);
    lay->setContentsMargins(12, 0, 0, 0);
    lay->setSpacing(8);
    auto* dot = new QLabel(titleBar_);
    dot->setFixedSize(10, 10);
    dot->setStyleSheet(QStringLiteral("background:#F57C00; border-radius:5px;"));
    lay->addWidget(dot);
    auto* title = new QLabel(QStringLiteral("RHOSTOOL  ·  Atelier Android / iOS"), titleBar_);
    title->setObjectName(QStringLiteral("TitleText"));
    lay->addWidget(title);
    lay->addStretch();
    auto mk = [&](const QString& t, const char* obj, auto slot) {
        auto* b = new QPushButton(t, titleBar_);
        b->setObjectName(QString::fromUtf8(obj));
        connect(b, &QPushButton::clicked, this, slot);
        lay->addWidget(b);
        return b;
    };
    mk(QStringLiteral("—"), "CaptionBtn", [this] { showMinimized(); });
    maxBtn_ = mk(QStringLiteral("❐"), "CaptionBtn", [this] { toggleMaxRestore(); });
    mk(QStringLiteral("✕"), "CaptionClose", [this] { close(); });
    return titleBar_;
}

QWidget* MainWindow::buildBrandGrid() {
    auto* strip = new QWidget(this);
    strip->setObjectName(QStringLiteral("BrandStrip"));
    strip->setFixedHeight(138);
    brandGrid_ = new QGridLayout(strip);
    brandGrid_->setContentsMargins(12, 8, 12, 8);
    brandGrid_->setHorizontalSpacing(7);
    brandGrid_->setVerticalSpacing(7);
    for (int c = 0; c < 10; ++c) brandGrid_->setColumnStretch(c, 1);
    for (int r = 0; r < 3; ++r) brandGrid_->setRowStretch(r, 1);
    return strip;
}

QWidget* MainWindow::buildLeftPanel() {
    auto* panel = new QWidget(this);
    panel->setObjectName(QStringLiteral("LeftPanel"));
    auto* lay = new QVBoxLayout(panel);
    lay->setContentsMargins(8, 10, 8, 6);
    lay->setSpacing(8);
    modeTabsHost_ = new QWidget(panel);
    modeTabsHost_->setLayout(new QHBoxLayout());
    modeTabsHost_->layout()->setContentsMargins(0, 0, 0, 0);
    lay->addWidget(modeTabsHost_);

    auto* searchRow = new QHBoxLayout();
    search_ = new QLineEdit(panel);
    search_->setPlaceholderText(QStringLiteral("Search model…"));
    connect(search_, &QLineEdit::textChanged, this, &MainWindow::filterModels);
    modelCount_ = new QLabel(QStringLiteral("0"), panel);
    modelCount_->setStyleSheet(QStringLiteral(
        "QLabel { background:#141414; border:1px solid #F57C00; border-radius:6px; padding:6px 10px; color:#FFB74D; font-weight:800; }"));
    searchRow->addWidget(search_, 1);
    searchRow->addWidget(modelCount_);
    lay->addLayout(searchRow);

    models_ = new QListWidget(panel);
    connect(models_, &QListWidget::currentRowChanged, this, [this](int) { onModelSelected(); });
    lay->addWidget(models_, 1);
    return panel;
}

QWidget* MainWindow::buildCenterPanel() {
    auto* panel = new QWidget(this);
    panel->setObjectName(QStringLiteral("CenterPanel"));
    auto* lay = new QVBoxLayout(panel);
    lay->setContentsMargins(12, 10, 12, 10);
    lay->setSpacing(10);

    funcTabsBar_ = new QWidget(panel);
    funcTabsBar_->setLayout(new QHBoxLayout());
    funcTabsBar_->layout()->setContentsMargins(0, 0, 0, 0);
    auto* tabsScroll = new QScrollArea(panel);
    tabsScroll->setWidgetResizable(true);
    tabsScroll->setHorizontalScrollBarPolicy(Qt::ScrollBarAsNeeded);
    tabsScroll->setVerticalScrollBarPolicy(Qt::ScrollBarAlwaysOff);
    tabsScroll->setFixedHeight(44);
    tabsScroll->setWidget(funcTabsBar_);
    lay->addWidget(tabsScroll);

    auto* modelBox = new QFrame(panel);
    modelBox->setStyleSheet(QStringLiteral("QFrame { background:#1F1F1F; border:1px solid #333; border-radius:7px; }"));
    auto* mb = new QHBoxLayout(modelBox);
    mb->setContentsMargins(12, 8, 12, 8);
    mb->setSpacing(8);
    modelLabel_ = new QLabel(QStringLiteral("Model: —"), modelBox);
    chipCombo_ = new QComboBox(modelBox);
    chipCombo_->setMinimumWidth(100);
    mb->addWidget(modelLabel_, 1);
    mb->addWidget(new QLabel(QStringLiteral("Chip"), modelBox));
    mb->addWidget(chipCombo_);
    lay->addWidget(modelBox);

    auto* scroll = new QScrollArea(panel);
    scroll->setWidgetResizable(true);
    scroll->setObjectName(QStringLiteral("OpsScroll"));
    opsHost_ = new QWidget(scroll);
    opsHost_->setObjectName(QStringLiteral("OpsHost"));
    auto* opsLay = new QVBoxLayout(opsHost_);
    opsLay->setContentsMargins(0, 0, 0, 0);
    opsLay->setSpacing(0);
    opsLay->setAlignment(Qt::AlignTop);
    scroll->setWidget(opsHost_);
    lay->addWidget(scroll, 1);
    return panel;
}

QWidget* MainWindow::buildRightPanel() {
    auto* panel = new QWidget(this);
    panel->setObjectName(QStringLiteral("RightPanel"));
    auto* lay = new QVBoxLayout(panel);
    lay->setContentsMargins(10, 10, 10, 10);
    lay->setSpacing(10);

    auto* utils = new QHBoxLayout();
    utils->setSpacing(8);
    auto mkUtil = [&](const QString& text, auto fn) {
        auto* b = new QPushButton(text, panel);
        b->setObjectName(QStringLiteral("UtilBtn"));
        connect(b, &QPushButton::clicked, this, fn);
        utils->addWidget(b);
    };
    mkUtil(QStringLiteral("ADB"), [this] { scanDevices(); });
    mkUtil(QStringLiteral("FAST\nBOOT"), [this] {
        logLine(adb_.run({QStringLiteral("devices"), QStringLiteral("-l")}));
    });
    mkUtil(QStringLiteral("T.POINT"), [this] { logLine(QStringLiteral("CONFIG AppData\\RhosTool")); });
    mkUtil(QStringLiteral("DEVMGR"), [this] {
        QProcess::startDetached(QStringLiteral("mmc.exe"), {QStringLiteral("devmgmt.msc")});
        logLine(QStringLiteral("Gestionnaire de périphériques ouvert."));
    });
    mkUtil(QStringLiteral("CONFIG"), [this] { openConfigDialog(); });
    lay->addLayout(utils);

    auto* usbRow = new QHBoxLayout();
    usbLabel_ = new QLabel(QStringLiteral("USB  —"), panel);
    usbLabel_->setStyleSheet(QStringLiteral("QLabel { background:#141414; border:1px solid #3A3A3A; border-radius:6px; padding:7px 10px; color:#CFCFCF; }"));
    auto* com = new QComboBox(panel);
    com->addItem(QStringLiteral("COM"));
    auto* fast = new QCheckBox(QStringLiteral("Fast"), panel);
    usbRow->addWidget(usbLabel_, 1);
    usbRow->addWidget(com);
    usbRow->addWidget(fast);
    lay->addLayout(usbRow);

    auto* consBox = new QFrame(panel);
    consBox->setStyleSheet(QStringLiteral("QFrame { border:1px solid #2E2E2E; border-radius:7px; background:#0E0E0E; }"));
    auto* cl = new QVBoxLayout(consBox);
    cl->setContentsMargins(10, 8, 10, 10);
    cl->setSpacing(6);
    auto* ch = new QHBoxLayout();
    auto* ct = new QLabel(QStringLiteral("CONSOLE"), consBox);
    ct->setObjectName(QStringLiteral("Accent"));
    catalogStats_ = new QLabel(consBox);
    catalogStats_->setStyleSheet(QStringLiteral("color:#8A8A8A; font-size:10px; font-weight:600;"));
    ch->addWidget(ct);
    ch->addStretch();
    ch->addWidget(catalogStats_);
    cl->addLayout(ch);
    console_ = new QPlainTextEdit(consBox);
    console_->setObjectName(QStringLiteral("Console"));
    console_->setReadOnly(true);
    cl->addWidget(console_, 1);
    lay->addWidget(consBox, 1);

    auto* bot = new QHBoxLayout();
    progress_ = new QProgressBar(panel);
    progress_->setRange(0, 100);
    progress_->setValue(0);
    auto* stop = new QPushButton(QStringLiteral("STOP"), panel);
    stop->setObjectName(QStringLiteral("StopBtn"));
    connect(stop, &QPushButton::clicked, this, [this] { progress_->setValue(0); logLine(QStringLiteral("STOP")); });
    bot->addWidget(progress_, 1);
    bot->addWidget(stop);
    lay->addLayout(bot);
    return panel;
}

QWidget* MainWindow::buildStatusBar() {
    auto* bar = new QWidget(this);
    bar->setObjectName(QStringLiteral("StatusBar"));
    bar->setFixedHeight(26);
    auto* lay = new QHBoxLayout(bar);
    lay->setContentsMargins(8, 0, 8, 0);
    statusLeft_ = new QLabel(QStringLiteral("Local · prêt"), bar);
    statusCredits_ = new QLabel(QStringLiteral("Crédits —"), bar);
    statusCredits_->setObjectName(QStringLiteral("Accent"));
    statusUser_ = new QLabel(qEnvironmentVariable("USERNAME", QStringLiteral("User")), bar);
    statusUser_->setObjectName(QStringLiteral("Accent"));
    statusClock_ = new QLabel(bar);
    lay->addWidget(statusLeft_, 1);
    lay->addWidget(statusCredits_);
    lay->addWidget(statusUser_);
    lay->addWidget(statusClock_);
    return bar;
}

void MainWindow::selectBrand(int index) {
    if (index < 0 || index >= brands_.size()) return;
    brandIndex_ = index;
    for (int i = 0; i < brandButtons_.size(); ++i)
        brandButtons_[i]->setChecked(i == index);
    brandModels_ = CatalogStore::instance().modelsForBrand(brands_[index].id);
    rebuildModeTabs();
    rebuildFuncTabs();
    search_->clear();
    rebuildModels();
    selectFunctionTab(0);
    logLine(QStringLiteral("Marque : %1 (%2 modèles)")
                .arg(brands_[index].shortName)
                .arg(brandModels_.size()));
}

void MainWindow::rebuildModeTabs() {
    auto* lay = qobject_cast<QHBoxLayout*>(modeTabsHost_->layout());
    if (!lay) return;
    QLayoutItem* child;
    while ((child = lay->takeAt(0)) != nullptr) {
        delete child->widget();
        delete child;
    }
    const auto& modes = brands_[brandIndex_].modes;
    for (int i = 0; i < modes.size(); ++i) {
        auto* b = new QPushButton(modes[i], modeTabsHost_);
        b->setObjectName(QStringLiteral("ModeTab"));
        b->setCheckable(true);
        b->setChecked(i == 0);
        connect(b, &QPushButton::clicked, this, [this, i] {
            const auto buttons = modeTabsHost_->findChildren<QPushButton*>();
            for (int j = 0; j < buttons.size(); ++j) buttons[j]->setChecked(j == i);
        });
        lay->addWidget(b);
    }
    lay->addStretch();
}

void MainWindow::openConfigDialog() {
    if (brandIndex_ < 0 || brandIndex_ >= brands_.size()) return;
    const auto& brand = brands_[brandIndex_];

    QDialog dlg(this);
    dlg.setWindowTitle(QStringLiteral("CONFIG — Ressources & Pilotes"));
    dlg.setObjectName(QStringLiteral("Root"));
    dlg.resize(620, 640);

    auto* v = new QVBoxLayout(&dlg);
    v->setContentsMargins(14, 14, 14, 14);
    v->setSpacing(10);

    auto* header = new QLabel(QStringLiteral("Ressources — pilotes, packs & outils (MediaFire)"), &dlg);
    header->setObjectName(QStringLiteral("Accent"));
    v->addWidget(header);

    auto* scroll = new QScrollArea(&dlg);
    scroll->setWidgetResizable(true);
    auto* host = new QWidget(scroll);
    auto* hv = new QVBoxLayout(host);
    hv->setContentsMargins(2, 2, 2, 2);
    hv->setSpacing(12);

    int groups = 0;
    for (const auto& tab : brand.tabs) {
        if (!isResourceTab(tab.title)) continue;
        ++groups;
        auto* lbl = new QLabel(tab.title, host);
        lbl->setObjectName(QStringLiteral("Accent"));
        hv->addWidget(lbl);

        auto* box = new QWidget(host);
        auto* flow = new FlowLayout(box, 0, 8, 8);
        for (const auto& op : tab.ops) {
            auto* b = new QPushButton(op.title, box);
            b->setObjectName(QStringLiteral("ActionBtn"));
            const CatalogOp cop = op;
            connect(b, &QPushButton::clicked, &dlg, [this, cop] { runCatalogOp(cop); });
            flow->addWidget(b);
        }
        box->setLayout(flow);
        hv->addWidget(box);
    }
    if (groups == 0)
        hv->addWidget(new QLabel(QStringLiteral("Aucune ressource pour cette marque."), host));
    hv->addStretch();
    host->setLayout(hv);
    scroll->setWidget(host);
    v->addWidget(scroll, 1);

    auto* close = new QPushButton(QStringLiteral("Fermer"), &dlg);
    close->setObjectName(QStringLiteral("UtilBtn"));
    connect(close, &QPushButton::clicked, &dlg, &QDialog::accept);
    v->addWidget(close);

    dlg.exec();
}

void MainWindow::consolidateTabs() {
    // Regroupe les fonctions ADB/Android (ADB TOOLS, MTP FILES, ADB APP CONTROL)
    // en un seul onglet « ANDROID » pour alléger la barre d'onglets.
    const QStringList mergeTitles = {
        QStringLiteral("ADB TOOLS"), QStringLiteral("MTP FILES"), QStringLiteral("ADB APP CONTROL")};
    for (auto& brand : brands_) {
        CatalogTab android;
        android.id = QStringLiteral("android-tools");
        android.title = QStringLiteral("ANDROID");
        int insertAt = -1;
        QVector<CatalogTab> kept;
        for (const auto& tab : brand.tabs) {
            if (mergeTitles.contains(tab.title, Qt::CaseInsensitive)) {
                for (const auto& op : tab.ops) android.ops.push_back(op);
                if (insertAt < 0) insertAt = kept.size();
            } else {
                kept.push_back(tab);
            }
        }
        if (!android.ops.isEmpty()) {
            if (insertAt < 0 || insertAt > kept.size()) insertAt = kept.size();
            kept.insert(insertAt, android);
        }
        brand.tabs = kept;
    }
}

void MainWindow::rebuildFuncTabs() {
    auto* lay = qobject_cast<QHBoxLayout*>(funcTabsBar_->layout());
    if (!lay) return;
    QLayoutItem* child;
    while ((child = lay->takeAt(0)) != nullptr) {
        delete child->widget();
        delete child;
    }
    funcTabButtons_.clear();
    const auto& brand = brands_[brandIndex_];
    const CatalogModel* model = currentModel();
    for (int i = 0; i < brand.tabs.size(); ++i) {
        if (!tabVisible(brand.tabs[i], brand, model)) continue;
        auto* b = new QPushButton(brand.tabs[i].title, funcTabsBar_);
        b->setObjectName(QStringLiteral("FuncTab"));
        b->setCheckable(true);
        b->setProperty("tabIndex", i);
        const int idx = i;
        connect(b, &QPushButton::clicked, this, [this, idx] { selectFunctionTab(idx); });
        lay->addWidget(b);
        funcTabButtons_.push_back(b);
    }
    lay->addStretch();
}

void MainWindow::selectFunctionTab(int index) {
    const auto& tabs = brands_[brandIndex_].tabs;
    if (tabs.isEmpty() || funcTabButtons_.isEmpty()) return;
    // Map to a currently visible tab; fall back to the first visible one.
    bool visible = false;
    for (auto* b : funcTabButtons_)
        if (b->property("tabIndex").toInt() == index) { visible = true; break; }
    if (!visible) index = funcTabButtons_.first()->property("tabIndex").toInt();
    funcTabIndex_ = index;
    for (auto* b : funcTabButtons_)
        b->setChecked(b->property("tabIndex").toInt() == index);
    rebuildOps();
}

const CatalogModel* MainWindow::currentModel() const {
    if (!models_) return nullptr;
    auto* item = models_->currentItem();
    if (!item) return nullptr;
    const int idx = item->data(Qt::UserRole).toInt();
    if (idx < 0 || idx >= brandModels_.size()) return nullptr;
    return &brandModels_[idx];
}

bool MainWindow::isResourceTab(const QString& title) const {
    const QString t = title.toUpper();
    return t == QStringLiteral("ALL DRIVER")
        || t == QStringLiteral("TOOL ANDROID-PC")
        || t == QStringLiteral("RESOURCES")
        || t.startsWith(QStringLiteral("RAMDISK"));
}

bool MainWindow::tabVisible(const CatalogTab& tab, const CatalogBrand& brand,
                            const CatalogModel* /*model*/) const {
    const QString t = tab.title.toUpper();

    // Les ressources / téléchargements (pilotes, packs, ramdisk, outils PC)
    // vivent désormais dans le bouton CONFIG, pas dans la barre d'onglets.
    if (isResourceTab(t)) return false;

    // Les fonctions ADB génériques ne s'affichent que sous la marque « Android ».
    if (t == QStringLiteral("ANDROID")) {
        const bool genericAndroid =
            brand.shortName.compare(QStringLiteral("Android"), Qt::CaseInsensitive) == 0
            || brand.id.compare(QStringLiteral("android"), Qt::CaseInsensitive) == 0;
        return genericAndroid;
    }

    // Reste : onglets d'actions (GENERAL, SPECIAL, …) toujours visibles.
    return true;
}

void MainWindow::rebuildModels() {
    filterModels(search_->text());
}

void MainWindow::filterModels(const QString& text) {
    models_->clear();
    const QString q = text.trimmed();
    int shown = 0;
    for (int i = 0; i < brandModels_.size(); ++i) {
        const auto& m = brandModels_[i];
        const bool ok = q.isEmpty()
            || m.name.contains(q, Qt::CaseInsensitive)
            || m.code.contains(q, Qt::CaseInsensitive)
            || m.codesDisplay().contains(q, Qt::CaseInsensitive)
            || m.chip.contains(q, Qt::CaseInsensitive)
            || m.series.contains(q, Qt::CaseInsensitive);
        if (!ok) continue;
        auto* item = new QListWidgetItem(
            (m.series.isEmpty() ? QString() : m.series + QStringLiteral("\n"))
            + m.name + QStringLiteral("\n")
            + m.codesDisplay() + QStringLiteral(" — ") + m.chip,
            models_);
        item->setData(Qt::UserRole, i);
        ++shown;
    }
    modelCount_->setText(QString::number(shown));
    if (models_->count() > 0) {
        models_->setCurrentRow(0);
        onModelSelected();
    } else {
        modelLabel_->setText(QStringLiteral("Model: —"));
        chipCombo_->clear();
    }
}

void MainWindow::onModelSelected() {
    auto* item = models_->currentItem();
    if (!item) return;
    const int idx = item->data(Qt::UserRole).toInt();
    if (idx < 0 || idx >= brandModels_.size()) return;
    const auto& m = brandModels_[idx];
    modelLabel_->setText(QStringLiteral("Model: ") + m.name);
    chipCombo_->clear();
    chipCombo_->addItem(m.chip.isEmpty() ? QStringLiteral("—") : m.chip);
}

QIcon MainWindow::opBullet(const QString& kind) const {
    QColor c(0xF5, 0x7C, 0x00); // orange par défaut
    if (kind == QLatin1String("ReadInfo")) c = QColor(0x42, 0xA5, 0xF5);
    else if (kind == QLatin1String("Reboot") || kind == QLatin1String("RebootRecovery")) c = QColor(0x66, 0xBB, 0x6A);
    else if (kind == QLatin1String("AdbTools") || kind == QLatin1String("AdbAppControl") || kind == QLatin1String("MtpFiles")) c = QColor(0x26, 0xC6, 0xDA);
    else if (kind == QLatin1String("FirmwarePack")) c = QColor(0xFF, 0xB3, 0x00);
    else if (kind == QLatin1String("OpenResourceLink")) c = QColor(0xAB, 0x47, 0xBC);
    else if (kind == QLatin1String("PartnerStub")) c = QColor(0x8D, 0x8D, 0x8D);
    else if (kind == QLatin1String("DeviceAction")) c = QColor(0xFF, 0x8A, 0x65);
    QPixmap pm(18, 18);
    pm.fill(Qt::transparent);
    QPainter p(&pm);
    p.setRenderHint(QPainter::Antialiasing);
    p.setPen(Qt::NoPen);
    p.setBrush(c.darker(130));
    p.drawEllipse(1, 1, 16, 16);
    p.setBrush(c);
    p.drawEllipse(3, 3, 12, 12);
    p.setBrush(QColor(255, 255, 255, 60));
    p.drawEllipse(5, 4, 8, 5);
    p.end();
    return QIcon(pm);
}

void MainWindow::rebuildOps() {
    auto* lay = qobject_cast<QVBoxLayout*>(opsHost_->layout());
    if (!lay) return;
    QLayoutItem* child;
    while ((child = lay->takeAt(0)) != nullptr) {
        if (child->widget()) child->widget()->deleteLater();
        delete child;
    }
    if (brands_.isEmpty()) return;
    const auto& tabs = brands_[brandIndex_].tabs;
    if (funcTabIndex_ < 0 || funcTabIndex_ >= tabs.size()) return;
    for (const auto& op : tabs[funcTabIndex_].ops) {
        auto* b = new QPushButton(QStringLiteral("  ") + op.title, opsHost_);
        b->setObjectName(QStringLiteral("OpRow"));
        b->setIcon(opBullet(op.kind));
        b->setIconSize(QSize(16, 16));
        b->setToolTip(op.description);
        b->setCursor(Qt::PointingHandCursor);
        b->setSizePolicy(QSizePolicy::Expanding, QSizePolicy::Fixed);
        b->setMinimumHeight(44);
        connect(b, &QPushButton::clicked, this, [this, op] { runCatalogOp(op); });
        lay->addWidget(b);
    }
    lay->addStretch();
}

QString MainWindow::askText(const QString& title, const QString& label, const QString& def) {
    bool ok = false;
    const auto t = QInputDialog::getText(this, title, label, QLineEdit::Normal, def, &ok);
    return ok ? t.trimmed() : QString();
}

void MainWindow::runCatalogOp(const CatalogOp& op) {
    logLine(QStringLiteral("> ") + op.title);
    progress_->setValue(20);

    if (op.kind == QLatin1String("PartnerStub") ||
        op.requiredMode == QLatin1String("Unavailable")) {
        logLine(QStringLiteral("ÉCHEC — module partenaire non exécuté en local."));
        progress_->setValue(0);
        return;
    }

    if (op.kind == QLatin1String("OpenResourceLink")) {
        const auto url = CatalogStore::instance().urlForOp(op.id);
        if (url.isEmpty()) {
            logLine(QStringLiteral("ÉCHEC — pas de lien MediaFire pour ") + op.id);
            progress_->setValue(0);
        } else {
            openUrl(url);
            progress_->setValue(100);
        }
        return;
    }

    if (op.kind == QLatin1String("OpenDeviceManager") || op.id == QLatin1String("drivers")) {
        QProcess::startDetached(QStringLiteral("mmc.exe"), {QStringLiteral("devmgmt.msc")});
        logLine(QStringLiteral("Gestionnaire de périphériques ouvert."));
        progress_->setValue(100);
        return;
    }

    if (op.kind == QLatin1String("FirmwarePack")) {
        if (op.id == QLatin1String("load-pack")) {
            const auto path = QFileDialog::getExistingDirectory(this, QStringLiteral("LOAD PACK — dossier firmware"));
            if (!path.isEmpty()) logLine(QStringLiteral("Pack sélectionné : ") + path);
            else logLine(QStringLiteral("LOAD PACK annulé."));
        } else {
            logLine(op.description);
        }
        progress_->setValue(100);
        return;
    }

    if (op.kind == QLatin1String("MtpFiles")) {
        logLine(QStringLiteral("MTP : ouverture Explorateur Windows (branchement MTP complet à suivre)."));
        QProcess::startDetached(QStringLiteral("explorer.exe"), {});
        progress_->setValue(100);
        return;
    }

    // ADB-related
    const bool needsDevice = op.requiresDevice &&
        op.requiredMode != QLatin1String("None");
    if (needsDevice && currentSerial_.isEmpty()) {
        logLine(QStringLiteral("ÉCHEC — aucun appareil ADB autorisé."));
        progress_->setValue(0);
        return;
    }

    if (op.kind == QLatin1String("ReadInfo") || op.id == QLatin1String("read-info") || op.id == QLatin1String("read-ios")) {
        logLine(adb_.readInfo(currentSerial_));
        progress_->setValue(100);
        return;
    }
    if (op.kind == QLatin1String("Reboot") || op.id == QLatin1String("reboot") || op.id == QLatin1String("adb-reboot") || op.id == QLatin1String("boot-device")) {
        logLine(adb_.runOn(currentSerial_, {QStringLiteral("reboot")}));
        logLine(QStringLiteral("Redémarrage demandé."));
        progress_->setValue(100);
        return;
    }
    if (op.kind == QLatin1String("RebootRecovery") || op.id.contains(QStringLiteral("recovery"))) {
        logLine(adb_.runOn(currentSerial_, {QStringLiteral("reboot"), QStringLiteral("recovery")}));
        logLine(QStringLiteral("Redémarrage recovery demandé."));
        progress_->setValue(100);
        return;
    }

    if (op.kind == QLatin1String("AdbTools")) {
        if (op.id == QLatin1String("adb-status") || op.id == QLatin1String("adb-devices")) {
            scanDevices();
            logLine(adb_.run({QStringLiteral("devices"), QStringLiteral("-l")}));
        } else if (op.id == QLatin1String("adb-state")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("get-state")}));
        } else if (op.id == QLatin1String("adb-wait")) {
            logLine(adb_.run({QStringLiteral("wait-for-device")}, 60000));
        } else if (op.id == QLatin1String("adb-tcpip")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("tcpip"), QStringLiteral("5555")}));
        } else if (op.id == QLatin1String("adb-connect")) {
            const auto ip = askText(QStringLiteral("CONNECT WIFI"), QStringLiteral("IP:port"), QStringLiteral("192.168.1.1:5555"));
            if (!ip.isEmpty()) logLine(adb_.run({QStringLiteral("connect"), ip}));
        } else if (op.id == QLatin1String("adb-disconnect")) {
            logLine(adb_.run({QStringLiteral("disconnect")}));
        } else if (op.id == QLatin1String("adb-ls")) {
            const auto path = askText(QStringLiteral("LIST"), QStringLiteral("Dossier distant"), QStringLiteral("/sdcard"));
            if (!path.isEmpty()) logLine(adb_.shell(currentSerial_, QStringLiteral("ls -la ") + path));
        } else if (op.id == QLatin1String("adb-shell")) {
            const auto cmd = askText(QStringLiteral("SHELL"), QStringLiteral("Commande"), QStringLiteral("getprop ro.product.model"));
            if (!cmd.isEmpty()) logLine(adb_.shell(currentSerial_, cmd));
        } else if (op.id == QLatin1String("adb-logcat")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("logcat"), QStringLiteral("-d"), QStringLiteral("-t"), QStringLiteral("200")}, 20000));
        } else if (op.id == QLatin1String("adb-screencap")) {
            const auto out = QFileDialog::getSaveFileName(this, QStringLiteral("SCREENSHOT"), QStringLiteral("screen.png"), QStringLiteral("PNG (*.png)"));
            if (!out.isEmpty()) {
                adb_.runOn(currentSerial_, {QStringLiteral("shell"), QStringLiteral("screencap"), QStringLiteral("-p"), QStringLiteral("/sdcard/rhostool.png")});
                logLine(adb_.runOn(currentSerial_, {QStringLiteral("pull"), QStringLiteral("/sdcard/rhostool.png"), out}));
            }
        } else if (op.id == QLatin1String("adb-reboot-bootloader")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("reboot"), QStringLiteral("bootloader")}));
        } else if (op.id == QLatin1String("adb-root")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("root")}));
        } else if (op.id == QLatin1String("adb-unroot")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("unroot")}));
        } else if (op.id == QLatin1String("adb-remount")) {
            logLine(adb_.runOn(currentSerial_, {QStringLiteral("remount")}));
        } else if (op.id == QLatin1String("adb-forward-list")) {
            logLine(adb_.run({QStringLiteral("forward"), QStringLiteral("--list")}));
        } else if (op.id == QLatin1String("adb-push")) {
            const auto local = QFileDialog::getOpenFileName(this, QStringLiteral("PUSH FILE"));
            if (!local.isEmpty()) {
                const auto remote = askText(QStringLiteral("PUSH"), QStringLiteral("Chemin distant"), QStringLiteral("/sdcard/"));
                if (!remote.isEmpty()) logLine(adb_.runOn(currentSerial_, {QStringLiteral("push"), local, remote}, 120000));
            }
        } else if (op.id == QLatin1String("adb-pull")) {
            const auto remote = askText(QStringLiteral("PULL"), QStringLiteral("Fichier distant"), QStringLiteral("/sdcard/"));
            if (!remote.isEmpty()) {
                const auto local = QFileDialog::getExistingDirectory(this, QStringLiteral("Dossier destination"));
                if (!local.isEmpty()) logLine(adb_.runOn(currentSerial_, {QStringLiteral("pull"), remote, local}, 120000));
            }
        } else {
            logLine(op.description);
            logLine(QStringLiteral("(ADB TOOLS — action générique)"));
        }
        progress_->setValue(100);
        return;
    }

    if (op.kind == QLatin1String("AdbAppControl")) {
        if (op.id == QLatin1String("list-pkgs") || op.id == QLatin1String("list-3rd")) {
            const auto args = op.id == QLatin1String("list-3rd")
                ? QStringList{QStringLiteral("shell"), QStringLiteral("pm"), QStringLiteral("list"), QStringLiteral("packages"), QStringLiteral("-3")}
                : QStringList{QStringLiteral("shell"), QStringLiteral("pm"), QStringLiteral("list"), QStringLiteral("packages")};
            logLine(adb_.runOn(currentSerial_, args, 60000));
        } else if (op.id == QLatin1String("install")) {
            const auto apk = QFileDialog::getOpenFileName(this, QStringLiteral("INSTALL APK"), {}, QStringLiteral("APK (*.apk)"));
            if (!apk.isEmpty()) logLine(adb_.runOn(currentSerial_, {QStringLiteral("install"), QStringLiteral("-r"), apk}, 180000));
        } else if (op.id == QLatin1String("uninstall") || op.id == QLatin1String("force-stop")
                   || op.id == QLatin1String("clear-data") || op.id == QLatin1String("enable-app")
                   || op.id == QLatin1String("disable-app") || op.id == QLatin1String("launch-app")
                   || op.id == QLatin1String("app-info") || op.id == QLatin1String("pull-apk")
                   || op.id == QLatin1String("clear-cache")) {
            const auto pkg = askText(op.title, QStringLiteral("Package (ex: com.android.chrome)"));
            if (pkg.isEmpty()) { progress_->setValue(0); return; }
            if (op.id == QLatin1String("uninstall"))
                logLine(adb_.runOn(currentSerial_, {QStringLiteral("uninstall"), pkg}));
            else if (op.id == QLatin1String("force-stop"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("am force-stop ") + pkg));
            else if (op.id == QLatin1String("clear-data"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("pm clear ") + pkg));
            else if (op.id == QLatin1String("enable-app"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("pm enable ") + pkg));
            else if (op.id == QLatin1String("disable-app"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("pm disable-user --user 0 ") + pkg));
            else if (op.id == QLatin1String("launch-app"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("monkey -p ") + pkg + QStringLiteral(" -c android.intent.category.LAUNCHER 1")));
            else if (op.id == QLatin1String("app-info"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("dumpsys package ") + pkg + QStringLiteral(" | head -n 40")));
            else if (op.id == QLatin1String("clear-cache"))
                logLine(adb_.shell(currentSerial_, QStringLiteral("pm clear --cache-only ") + pkg));
            else if (op.id == QLatin1String("pull-apk")) {
                const auto path = adb_.shell(currentSerial_, QStringLiteral("pm path ") + pkg).trimmed();
                auto apk = path;
                if (apk.startsWith(QStringLiteral("package:"))) apk = apk.mid(8);
                if (!apk.isEmpty()) {
                    const auto dest = QFileDialog::getExistingDirectory(this, QStringLiteral("PULL APK"));
                    if (!dest.isEmpty()) logLine(adb_.runOn(currentSerial_, {QStringLiteral("pull"), apk, dest}, 120000));
                } else logLine(QStringLiteral("APK introuvable."));
            }
        } else {
            logLine(op.description);
        }
        progress_->setValue(100);
        return;
    }

    if (op.kind == QLatin1String("DeviceAction")) {
        if (op.id == QLatin1String("factory") || op.id == QLatin1String("format-data")) {
            logLine(adb_.shell(currentSerial_, QStringLiteral("am broadcast -a android.intent.action.FACTORY_RESET")));
            logLine(QStringLiteral("Factory reset demandé (confirmation possible sur l'appareil)."));
        } else if (op.id == QLatin1String("screen-lock")) {
            logLine(adb_.shell(currentSerial_, QStringLiteral("am start -a android.settings.SECURITY_SETTINGS")));
        } else {
            logLine(op.description);
        }
        progress_->setValue(100);
        return;
    }

    logLine(op.description);
    progress_->setValue(100);
}

void MainWindow::scanDevices() {
    if (!adb_.available()) {
        usbLabel_->setText(QStringLiteral("USB  ADB absent"));
        currentSerial_.clear();
        statusLeft_->setText(QStringLiteral("Local · ADB absent"));
        logLine(QStringLiteral("ADB introuvable."));
        return;
    }
    const auto list = adb_.devices();
    if (list.isEmpty()) {
        usbLabel_->setText(QStringLiteral("USB  —  ·  ADB"));
        currentSerial_.clear();
        statusLeft_->setText(QStringLiteral("Local · aucun appareil"));
        logLine(QStringLiteral("Scan : aucun appareil."));
        return;
    }
    const auto& d = list.first();
    currentSerial_ = d.authorized ? d.serial : QString();
    usbLabel_->setText(QStringLiteral("USB  ") + d.serial + QStringLiteral("  ·  ") + d.state);
    statusLeft_->setText(QStringLiteral("Local · ") + d.serial);
    logLine(QStringLiteral("Scan : ") + d.serial + QStringLiteral(" (") + d.state + QStringLiteral(")"));
    progress_->setValue(100);
}

void MainWindow::logLine(const QString& line) {
    const QString up = line.toUpper();
    QString color = QStringLiteral("#D4D4D4");
    if (up.contains(QStringLiteral("ERREUR")) || up.contains(QStringLiteral("ERROR"))
        || up.contains(QStringLiteral("ECHEC")) || up.contains(QStringLiteral("\u00c9CHEC"))
        || up.contains(QStringLiteral("FAIL")))
        color = QStringLiteral("#FF6E6E");
    else if (up.contains(QStringLiteral("SUCC")) || up.contains(QStringLiteral("TERMIN"))
             || line.contains(QChar(0x2713)) /* checkmark */)
        color = QStringLiteral("#9CCC65");
    else if (line.startsWith(QStringLiteral("Marque")) || line.startsWith(QStringLiteral("Catalogues"))
             || line.startsWith(QStringLiteral("Scan")) || line.startsWith(QStringLiteral(">"))
             || line.startsWith(QStringLiteral("==")))
        color = QStringLiteral("#FFB74D");
    console_->appendHtml(QStringLiteral("<span style=\"color:%1;\">%2</span>")
                             .arg(color, line.toHtmlEscaped()));
}
void MainWindow::openUrl(const QString& url) {
    QDesktopServices::openUrl(QUrl(url));
    logLine(QStringLiteral("Ouvert : ") + url);
}
void MainWindow::updateClock() {
    statusClock_->setText(QDateTime::currentDateTime().toString(QStringLiteral("HH:mm:ss")));
}
void MainWindow::toggleMaxRestore() {
    if (isMaximized()) showNormal();
    else showMaximized();
}

void MainWindow::mousePressEvent(QMouseEvent* e) {
    if (e->button() == Qt::LeftButton && titleBar_ && titleBar_->geometry().contains(e->pos())) {
        dragging_ = true;
        dragPos_ = e->globalPosition().toPoint() - frameGeometry().topLeft();
        e->accept();
        return;
    }
    QWidget::mousePressEvent(e);
}
void MainWindow::mouseMoveEvent(QMouseEvent* e) {
    if (dragging_ && (e->buttons() & Qt::LeftButton) && !isMaximized()) {
        move(e->globalPosition().toPoint() - dragPos_);
        e->accept();
        return;
    }
    QWidget::mouseMoveEvent(e);
}
void MainWindow::mouseReleaseEvent(QMouseEvent* e) {
    dragging_ = false;
    QWidget::mouseReleaseEvent(e);
}
void MainWindow::mouseDoubleClickEvent(QMouseEvent* e) {
    if (titleBar_ && titleBar_->geometry().contains(e->pos())) {
        toggleMaxRestore();
        e->accept();
        return;
    }
    QWidget::mouseDoubleClickEvent(e);
}
