#-------------------------------------------------
#
# Project created by QtCreator 2013-02-03T17:30:55
#
#-------------------------------------------------

QT       += core gui

TARGET = ImageTool
TEMPLATE = app


SOURCES += main.cpp\
        mainwindow.cpp \
    image.cpp

HEADERS  += mainwindow.h \
    image.h \
    logger.h

FORMS    += mainwindow.ui

INCLUDEPATH += "C:/Lab/1GAM_01_Aries/Aries/Dev Tools/ImageTool/devIL/include/IL"

LIBS += "C:/Lab/1GAM_01_Aries/Aries/Dev Tools/ImageTool/devIL/lib/DevIL.lib" \
        "C:/Lab/1GAM_01_Aries/Aries/Dev Tools/ImageTool/devIL/lib/ILU.lib" \
        "C:/Lab/1GAM_01_Aries/Aries/Dev Tools/ImageTool/devIL/lib/ILUT.lib"
