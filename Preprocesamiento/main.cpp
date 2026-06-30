#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <ctime>
#include <string>
#include <iostream>
#include <dirent.h>

using namespace std;
using namespace cv;


void randomRotation(Mat img, Mat* imgSet, int n, int s, int t){
    int x = img.cols / 2;
    int y = img.rows / 2;
    double angle;
    imgSet[0] = img;
    imgSet[0] = imgSet[0](Rect(imgSet[0].cols/2-s/2, imgSet[0].rows/2 - s/2, s, s));
        resize(imgSet[0], imgSet[0], Size(t, t), 0, 0, INTER_LINEAR);
    for(int i = 1; i < n; i++){
        angle = (rand() % (90 - 10) + 10) + (90 * (i - 1));
        warpAffine(img, imgSet[i], getRotationMatrix2D(Point2f(x,y), angle, 1.0), Size(img.cols, img.rows) );
        imgSet[i] = imgSet[i](Rect(imgSet[i].cols/2-s/2, imgSet[i].rows/2 - s/2, s, s));
        resize(imgSet[i], imgSet[i], Size(t, t), 0, 0, INTER_LINEAR);
    }
}
int main(int argc, char *argv[])
{
    srand(time(NULL));
    const char* srcPath = "images/";
    string pathMad = "C:/Users/JLuis/Documents/dataset/maduras/";
    string pathVer = "C:/Users/JLuis/Documents/dataset/verdes/";
    string destName, imgName, imgWrt;
    int nm = 1, nv = 1;
    DIR *dir;
    struct dirent *ent;
    if ((dir = opendir (srcPath)) != NULL) {
        cout << "Procesando imagenes...." << endl;
      while ((ent = readdir (dir)) != NULL) {
        imgName = ent->d_name;
        int s = imgName.size();

        if(s > 4){
            string ext = imgName.substr(s-4,4);
            if(ext == ".jpg" || "jpeg"){
                imgWrt = srcPath + imgName;
                Mat img = imread(imgWrt, IMREAD_COLOR);
                Mat rotated [5];
                int sectionSize = 300;
                int tam = 224;

                randomRotation(img, rotated, 5, sectionSize, tam);

                for(int i = 0; i < 5; i++){
                    if(imgName.at(0) == 'M' || imgName.at(0) == 'm'){
                        destName = pathMad + "madura_" + to_string(nm) + ".jpg";
                        nm++;
                    }else{
                        destName = pathVer + "verde_" + to_string(nv) + ".jpg";
                        nv++;
                    }
                    imwrite(destName, rotated[i]);
                }
            }
        }
      }
      cout << "Imagenes guardadas en: C:/Users/JLuis/Documents/dataset/" << endl;
      closedir (dir);
    } else {
      perror ("");
      return EXIT_FAILURE;
    }
    return 0;
}



