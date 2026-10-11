#pragma once

#include <QLayout>
#include <QRect>
#include <QStyle>
#include <QWidgetItem>

/** Disposition type WrapPanel (comme la version WPF). */
class FlowLayout : public QLayout {
public:
    explicit FlowLayout(QWidget* parent = nullptr, int margin = 0, int h = 8, int v = 8)
        : QLayout(parent), hSpace_(h), vSpace_(v) {
        setContentsMargins(margin, margin, margin, margin);
    }
    ~FlowLayout() override { QLayoutItem* i; while ((i = takeAt(0))) delete i; }

    void addItem(QLayoutItem* item) override { items_.append(item); }
    int count() const override { return items_.size(); }
    QLayoutItem* itemAt(int i) const override { return (i >= 0 && i < items_.size()) ? items_[i] : nullptr; }
    QLayoutItem* takeAt(int i) override { return (i >= 0 && i < items_.size()) ? items_.takeAt(i) : nullptr; }
    Qt::Orientations expandingDirections() const override { return {}; }
    bool hasHeightForWidth() const override { return true; }
    int heightForWidth(int w) const override { return doLayout(QRect(0, 0, w, 0), true); }
    void setGeometry(const QRect& r) override { QLayout::setGeometry(r); doLayout(r, false); }
    QSize sizeHint() const override { return minimumSize(); }
    QSize minimumSize() const override {
        QSize s;
        for (auto* i : items_) s = s.expandedTo(i->minimumSize());
        const auto m = contentsMargins();
        s += QSize(m.left() + m.right(), m.top() + m.bottom());
        return s;
    }

private:
    int doLayout(const QRect& rect, bool testOnly) const {
        int x = rect.x(), y = rect.y(), lineH = 0;
        const auto m = contentsMargins();
        int effRight = rect.right() - m.right();
        x += m.left();
        y += m.top();
        for (auto* item : items_) {
            auto* w = item->widget();
            int spaceX = hSpace_, spaceY = vSpace_;
            if (w) {
                spaceX = w->style()->layoutSpacing(QSizePolicy::PushButton, QSizePolicy::PushButton, Qt::Horizontal);
                if (spaceX < 0) spaceX = hSpace_;
                spaceY = w->style()->layoutSpacing(QSizePolicy::PushButton, QSizePolicy::PushButton, Qt::Vertical);
                if (spaceY < 0) spaceY = vSpace_;
            }
            int nextX = x + item->sizeHint().width() + spaceX;
            if (nextX - spaceX > effRight && lineH > 0) {
                x = rect.x() + m.left();
                y = y + lineH + spaceY;
                nextX = x + item->sizeHint().width() + spaceX;
                lineH = 0;
            }
            if (!testOnly)
                item->setGeometry(QRect(QPoint(x, y), item->sizeHint()));
            x = nextX;
            lineH = qMax(lineH, item->sizeHint().height());
        }
        return y + lineH - rect.y() + m.bottom();
    }

    QList<QLayoutItem*> items_;
    int hSpace_;
    int vSpace_;
};
