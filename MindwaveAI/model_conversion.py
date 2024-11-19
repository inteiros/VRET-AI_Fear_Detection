import tensorflow as tf

model = tf.keras.models.load_model("eeg_model_optimized_crossval.h5", compile=False)

saved_model_path = "saved_model"
tf.saved_model.save(model, saved_model_path)

print(f"Modelo salvo no formato SavedModel em: {saved_model_path}")
