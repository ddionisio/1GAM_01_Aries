#include <QtGui/QApplication>
#include "mainwindow.h"

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    a.setApplicationName("Image Tools");
    a.setOrganizationName("Renegadeware");
    a.setOrganizationDomain("renegadeware.com");
    MainWindow w;
    w.show();

    return a.exec();
}
