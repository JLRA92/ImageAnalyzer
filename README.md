-----  IMAGE ANALYZER  -----

Interfaz que permite seleccionar un modelo previamente entrenado en formato .onnx y seleccioar una imagen para su clsificación, actualmente reconoce dos clases "FRUTA VERDE" o "FRUTA MADURA". Así mismo, el programa permite iniciar una cmara WEB para hacer la clasificación en tiempo real.

-----  MODEL DLL  -----

Crea el .dll necesario para conectar la interfaz con el modelo .onnx.

-----  WEBCAM OPENCV -----

Interfaz para captura de imágenes base para postreriormente formar el dataset. Al capturar cada imagen, permite clasificarla como "MADURA" o "VERDE", añadiendo un identificador que permitirá a la herramienta de preprocesamiento ordenar las imágenes del dataset.
