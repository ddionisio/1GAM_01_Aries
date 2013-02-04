#ifndef IMAGE_H
#define IMAGE_H

#include <QString>

// Note: Only process PNG files.

// Set RGB to 0 if alpha is 0
bool ImageRemoveRGBAlpha(const QString & filepath);

#endif // IMAGE_H
