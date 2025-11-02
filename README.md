# GPT-4o Radiologist Assistant Tool

This repository contains a tool that utilizes OpenAI's GPT-4o model with vision capabilities to assist radiologists in analyzing MRI images. The tool is designed to highlight key features and anomalies, serving as a supplementary tool for radiologists to aid in their workflow. And yes, this project is built using **LINQPad… because why not?**

## Overview

This tool provides:
- Analysis of MRI images to identify patterns, structures, and potential anomalies.
- Descriptions of tissue density differences, irregular structures, or areas that might need further review.
- Support for radiologists in identifying areas of interest, but it does **not provide medical diagnoses**.

## Why LINQPad?

This project is developed in **LINQPad** because:
- It offers a lightweight environment for rapid prototyping and testing.
- It allows quick iteration and real-time feedback without the overhead of a traditional IDE.
- Sometimes, you just want simplicity—**LINQPad… because why not?**

## How It Works

1. **Image Processing**: The tool accepts MRI images encoded in base64 format.
2. **Model Integration**: GPT-4o processes the image and highlights features such as tissue patterns, anomalies, or irregularities that may be relevant to the radiologist.
3. **Output**: The tool provides a description of the features in the MRI, assisting radiologists in identifying areas that may require further analysis.
4. **Image Upload**: The tool allows users to upload their own MRI images from a local directory for analysis.

## Installation and Usage

### Prerequisites

- LINQPad 5 or later (for running the C# code).
- OpenAI API Key with access to GPT-4o.

### Steps

1. Clone this repository
2. Open the `.linq` file in LINQPad.
3. Set up your **OpenAI API Key** in the environment variables or hardcode it in the script (for testing purposes).
4. Run the LINQPad script and, when prompted, provide the path to the directory containing your MRI images.
5. The script will then send the images to GPT-4o and retrieve descriptive insights.

## Example

Here’s a basic example of what the output might look like for an MRI image:

**Input**: MRI image (base64-encoded)

**Output**:  
- Descriptions of visible structures.
- Highlighted patterns or anomalies that require further investigation.
- General tissue density observations.

## Future Features

- [ ] **Real-time Image Analysis**: Implement a feature to analyze images as they are uploaded.
- [ ] **Batch Processing**: Allow users to upload multiple images at once for batch analysis.
- [ ] **Integration with PACS**: Integrate with PACS systems to directly pull images for analysis.
- [ ] **User Authentication**: Add user authentication to protect patient data.
- [ ] **Report Generation**: Generate a PDF report of the analysis.
- [ ] **Image Annotation**: Allow users to annotate images with their own notes and observations.

## Limitations

- This tool does **not** provide medical diagnoses and is not intended to replace the expertise of a trained radiologist. It is designed to assist by highlighting areas for further review.
