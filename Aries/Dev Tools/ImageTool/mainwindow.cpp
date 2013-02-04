#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <QSettings>

#include <QtDebug>

#include <QFileDialog>

#include "logger.h"
#include "image.h"

#include "il.h"

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow),
    recentDir("")
{
    ui->setupUi(this);

    //logger
    Logger::Init(ui->log);

    //slots
    connect(ui->actionImageRemoveRGBDir, SIGNAL(triggered()), SLOT(removeRGBAlphaDir()));
    connect(ui->actionImageRemoveRGBFiles, SIGNAL(triggered()), SLOT(removeRGBAlphaFiles()));

    //devIL
    ilInit();

    ReadSettings();
}

MainWindow::~MainWindow()
{
    ilShutDown();

    Logger::Shutdown();

    delete ui;
}

void MainWindow::closeEvent(QCloseEvent *) {
    WriteSettings();
}

void DoRemoveRGBAlphaDir(const QString & pathDir) {
    QDir dir(pathDir);

    QStringList filters; filters << "*.png";

    QFileInfoList files = dir.entryInfoList(filters, QDir::Files | QDir::Readable | QDir::Writable);

    for(QFileInfoList::Iterator it = files.begin(); it != files.end(); it++) {
        QFileInfo info = *it;

        QString filepath = info.absoluteFilePath();

        if(ImageRemoveRGBAlpha(filepath)) {
            qDebug() << filepath << " process succeeded.";
        }
        else {
            qDebug() << filepath << " process failed.";
        }
    }

    QFileInfoList dirs = dir.entryInfoList(QDir::Dirs);

    for(QFileInfoList::Iterator it = dirs.begin(); it != dirs.end(); it++) {
        QFileInfo info = *it;
        if(info.baseName().count() > 0) {
            QString dirpath = info.absoluteFilePath();

            DoRemoveRGBAlphaDir(dirpath);
        }
    }
}

void MainWindow::removeRGBAlphaDir() {
    QString path = QFileDialog::getExistingDirectory(this, "Select a folder to process PNG images.", recentDir);

    recentDir = path;

    DoRemoveRGBAlphaDir(path);
}

void MainWindow::removeRGBAlphaFiles() {
    QStringList paths = QFileDialog::getOpenFileNames(this, "Select PNGs to process.", recentDir, "*.png");

    bool gotDir = false;

    for(QStringList::Iterator it = paths.begin(); it != paths.end(); it++) {
        if(!gotDir) {
            QDir dir = QFileInfo(*it).absoluteDir();
            recentDir = dir.absolutePath();
            gotDir = true;
        }

        if(ImageRemoveRGBAlpha(*it)) {
            qDebug() << *it << " process succeeded.";
        }
        else {
            qDebug() << *it << " process failed.";
        }
    }
}

void MainWindow::ReadSettings() {
    QSettings settings("Renegadeware", "Image Tools");

    recentDir = settings.value("recentDir").toString();
}

void MainWindow::WriteSettings() {
    QSettings settings("Renegadeware", "Image Tools");

    settings.setValue("recentDir", recentDir);
}
