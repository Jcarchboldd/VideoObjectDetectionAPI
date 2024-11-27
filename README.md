# **Video Object Detection API**

## **Overview**
The **Video Object Detection API** is a RESTful service that processes video files, extracts frames, and identifies objects or animals in each frame using the pre-trained **GoogLeNet ONNX model**. The API returns metadata about detected objects, including names, confidence scores, and timestamps.

You can upload a video directly through the **Swagger UI**, test the API, and receive detailed results in JSON format.

---

## **Features**
- Upload videos through Swagger.
- Extract keyframes from videos at regular intervals.
- Detect objects or animals in frames using GoogLeNet.
- Return metadata with:
  - Object name
  - Confidence score
  - Frame file path

---

## **Endpoints**

### **POST** `/api/VideoProcessing/process`
Uploads a video, processes it, and returns detection metadata.

- **Request**:  
  - **Content-Type**: `multipart/form-data`
  - **Parameters**:
    - `videoFile` (IFormFile): The video file to be analyzed.

- **Response**:  
Returns metadata for each detected object.  
Example:
```json
{
  "message": "Video processed successfully.",
  "description": "Video_Test_1",
  "results": [
    {
      "objectName": "gibbon",
      "confidenceScore": 0.0025812387,
      "frameFilePath": "/var/folders/ht/q9dxy3j92wl83tjcv23kp3ww0000gp/T/ExtractedFrames/frame_420.jpg"
    },
    {
      "objectName": "marmot",
      "confidenceScore": 0.0011718039,
      "frameFilePath": "/var/folders/ht/q9dxy3j92wl83tjcv23kp3ww0000gp/T/ExtractedFrames/frame_0.jpg"
    }
  ]
}
```

## **Setup**

### **Prerequisites**
1. Install .NET SDK 9.0 or later.
2. Download the pre-trained GoogLeNet ONNX model and `imagenet-simple-labels.json` file.

### **Steps**
1. Clone the repository:
   git clone https://github.com/Jcarchboldd/VideoObjectDetectionAPI.git

2. Restore dependencies:
   dotnet restore

3. Run the API:
   dotnet run --project src/WebApp

4. Access the Swagger UI by navigating to:
   http://localhost:5150/swagger/index.html


## **Testing**

### **Example Video**
Use the following sample video to test the API:  
[Video by Magda Ehlers from Pexels](https://www.pexels.com/video/footage-of-a-primate-eating-3765234/)

Alternatively, upload any video file of your choice through the Swagger UI or API.

