#ifndef LOGGER_H
#define LOGGER_H

#include <qapplication.h>
#include <QString>
#include <QListWidget>
#include <QtDebug>

class Logger {
public:
    static void Init(QListWidget *listView);

    static void Shutdown() { mListView = 0; }

private:
    static QListWidget *mListView;

    friend void LogOutput(QtMsgType type, const char *msg);
};

QListWidget *Logger::mListView = 0;

void LogOutput(QtMsgType type, const char *msg) {
    switch(type) {
    default:
        Logger::mListView->addItem(QString(msg));
        break;
    }
}

void Logger::Init(QListWidget *listView) {
    mListView = listView;

    qInstallMsgHandler(LogOutput);
}

#endif // LOGGER_H
