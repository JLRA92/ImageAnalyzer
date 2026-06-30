#include <opencv2/opencv.hpp>
#include <opencv2/dnn.hpp>

using namespace cv;
using namespace cv::dnn;

static Net net;

extern "C" {
    __declspec(dllexport) bool CargarModelo(const char* modelPath) {
        try {
            net = readNetFromONNX(modelPath);
            if (net.empty())
                return false;
            return true;
        }
        catch (...) {
            return false;
        }
    }
    __declspec(dllexport) bool ClasificarImagen(const char* imagePath, int* outClassId, float* outConfidence) {
        if (net.empty() || imagePath == nullptr || outClassId == nullptr || outConfidence == nullptr) {
            return false;
        }
        try {
            Mat frame = imread(imagePath);
            if (frame.empty())
                return false;
            Mat resized, rgbFrame, floatFrame;
            resize(frame, resized, Size(224, 224));
            cvtColor(resized, rgbFrame, COLOR_BGR2RGB);
            rgbFrame.convertTo(floatFrame, CV_32F);
            int sizes[] = {1, 224, 224, 3};
            Mat blobNHWC(4, sizes, CV_32F, floatFrame.data);
            net.setInput(blobNHWC);
            Mat prob = net.forward();
            double minVal, maxVal;
            Point classIdPoint;
            minMaxLoc(prob, &minVal, &maxVal, &classIdPoint, 0);
            *outClassId = classIdPoint.x;
            *outConfidence = (float)maxVal;
            return true;
        }
        catch (...) {
            return false;
        }
    }
    __declspec(dllexport) bool ClasificarFotograma(unsigned char* pixelBuffer, int width, int height, int channels, int* outClassId, float* outConfidence) {
        if (net.empty() || pixelBuffer == nullptr || outClassId == nullptr || outConfidence == nullptr) {
            return false;
        }

        try {
            int type = (channels == 3) ? CV_8UC3 : CV_8UC4;
            Mat frame(height, width, type, pixelBuffer);
            Mat bgrFrame;
            if (channels == 4) {
                cvtColor(frame, bgrFrame, COLOR_BGRA2BGR);
            } else {
                bgrFrame = frame;
            }
            Mat resized, rgbFrame, floatFrame;
            resize(bgrFrame, resized, Size(224, 224));
            cvtColor(resized, rgbFrame, COLOR_BGR2RGB);
            rgbFrame.convertTo(floatFrame, CV_32F);
            int sizes[] = {1, 224, 224, 3};
            Mat blobNHWC(4, sizes, CV_32F, floatFrame.data);
            net.setInput(blobNHWC);
            Mat prob = net.forward();
            double minVal, maxVal;
            Point classIdPoint;
            minMaxLoc(prob, &minVal, &maxVal, &classIdPoint, 0);
            *outClassId = classIdPoint.x;
            *outConfidence = (float)maxVal;
            return true;
        }
        catch (...) {
            return false;
        }
    }
}
