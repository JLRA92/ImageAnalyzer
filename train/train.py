import tensorflow as tf
from tensorflow.keras import layers, models
import os

IMG_SIZE = (224, 224) 
BATCH_SIZE = 32 
DATA_DIR = r'C:\Users\JLuis\Documents\dataset' 

train_ds = tf.keras.utils.image_dataset_from_directory(
    DATA_DIR,
    validation_split=0.2,
    subset="training",
    seed=123,
    image_size=IMG_SIZE,
    batch_size=BATCH_SIZE) 
print("Orden de las clases:", train_ds.class_names)
val_ds = tf.keras.utils.image_dataset_from_directory(
    DATA_DIR,
    validation_split=0.2,
    subset="validation",
    seed=123,
    image_size=IMG_SIZE,
    batch_size=BATCH_SIZE) 

model = models.Sequential([
    layers.Input(shape=(224, 224, 3)), 
    layers.Rescaling(1.0/127.5, offset=-1.0),
    layers.Conv2D(32, (3, 3), activation='relu'),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(64, (3, 3), activation='relu'),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(128, (3, 3), activation='relu'),
    layers.MaxPooling2D((2, 2)),
    layers.Conv2D(128, (3, 3), activation='relu'),
    layers.GlobalAveragePooling2D(),
    layers.Dropout(0.25),
    layers.Dense(2, activation='softmax')
])

model.compile(optimizer=tf.keras.optimizers.Adam(learning_rate=0.001),
              loss='sparse_categorical_crossentropy',
              metrics=['accuracy'])
model.fit(train_ds, validation_data=val_ds, epochs=12)

print("Guardando modelo...")
model.export('modelo')
os.system('python -m tf2onnx.convert --saved-model modelo --output modelo.onnx --opset 11')