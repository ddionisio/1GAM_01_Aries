#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>

namespace Ui {
    class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = 0);
    ~MainWindow();

public slots:
    void removeRGBAlphaDir();
    void removeRGBAlphaFiles();

private:
    Ui::MainWindow *ui;

    QString recentDir;

    void closeEvent(QCloseEvent *);

    void ReadSettings();
    void WriteSettings();
};

#endif // MAINWINDOW_H
