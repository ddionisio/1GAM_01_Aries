#include "image.h"

#include <QtDebug>

#include "il.h"

bool ImageRemoveRGBAlpha(const QString & filepath) {
    bool ret = true;

    ILuint ImageName = ilGenImage();
    ilBindImage(ImageName);

    const char *cpath = filepath.toUtf8();

    if(ilLoad(IL_PNG, cpath)) {
        //only alpha
        switch(ilGetInteger(IL_IMAGE_FORMAT)) {
        case IL_RGBA:
            ILubyte *bits = ilGetData();

            ILint w = ilGetInteger(IL_IMAGE_WIDTH);
            ILint h = ilGetInteger(IL_IMAGE_HEIGHT);

            for(int y = 0; y < h; y++) {
                ILubyte *bitsRow = bits + y*w*4;

                for(int x = 0, ofs = 0; x < w; x++, ofs += 4) {
                    if(bitsRow[ofs+3] == 0) {
                        bitsRow[ofs] = bitsRow[ofs+1] = bitsRow[ofs+2] = 0;
                    }
                }
            }

            if(!ilEnable(IL_FILE_OVERWRITE)) {

                qDebug() << "file: " << filepath << " il error: " << ilGetError();
                ret = false;
            }

             if(!ilSaveImage(cpath)) {
                 qDebug() << "file: " << filepath << " il error: " << ilGetError();
                 ret = false;
             }
            break;
        }

        ilDeleteImage(ImageName);
    }
    else {
        qDebug() << "file: " << filepath << " il error: " << ilGetError();
        ret = false;
    }

    return ret;
}
