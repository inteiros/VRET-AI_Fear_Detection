import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split, KFold
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import classification_report, confusion_matrix
from imblearn.over_sampling import SMOTE
import tensorflow as tf
import matplotlib.pyplot as plt
import seaborn as sns
import json

file1 = "datasets/mindwave_session1_flagged.csv"
file2 = "datasets/mindwave_session_chill_flagged.csv"
file3 = "datasets/mindwave_session_flagged2.csv"

df1 = pd.read_csv(file1)
df2 = pd.read_csv(file2)
df3 = pd.read_csv(file3)

data = pd.concat([df1, df2, df3], ignore_index=True)

features = ['Delta', 'Theta', 'LowAlpha', 'HighAlpha', 'LowBeta', 'HighBeta', 'LowGamma', 'HighGamma', 'Attention', 'Meditation', 'EEGValue', 'BlinkStrength']
X = data[features]
y = data['State'].map({'chill': 0, 'fear': 1})

scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

scaler_params = {
    "means": scaler.mean_.tolist(),
    "stds": scaler.scale_.tolist()
}

with open("scaler_params.json", "w") as f:
    json.dump(scaler_params, f)

smote = SMOTE(random_state=42)
X_resampled, y_resampled = smote.fit_resample(X_scaled, y)

X_resampled = X_resampled[..., np.newaxis]

input_shape = (X_resampled.shape[1], 1)

model = tf.keras.models.Sequential([
    tf.keras.layers.Input(shape=input_shape),
    tf.keras.layers.Conv1D(128, 3, activation='relu'),
    tf.keras.layers.Conv1D(256, 3, activation='relu'),
    tf.keras.layers.MaxPooling1D(2),
    tf.keras.layers.Conv1D(256, 3, activation='relu'),
    tf.keras.layers.MaxPooling1D(2),
    tf.keras.layers.Flatten(),
    tf.keras.layers.Dense(128, activation='relu'),
    tf.keras.layers.Dropout(0.4),
    tf.keras.layers.Dense(64, activation='relu'),
    tf.keras.layers.Dropout(0.4),
    tf.keras.layers.Dense(1, activation='sigmoid')
])

optimizer = tf.keras.optimizers.Adam(learning_rate=0.0001)
model.compile(optimizer=optimizer, loss='binary_crossentropy', metrics=['accuracy'])

kf = KFold(n_splits=5, shuffle=True, random_state=42)
histories = []
confusion_matrices = []

for train_index, test_index in kf.split(X_resampled):
    X_train, X_test = X_resampled[train_index], X_resampled[test_index]
    y_train, y_test = y_resampled[train_index], y_resampled[test_index]

    history = model.fit(X_train, y_train, epochs=50, batch_size=64, validation_data=(X_test, y_test))
    histories.append(history)

    y_pred = (model.predict(X_test) > 0.5).astype(int)
    cm = confusion_matrix(y_test, y_pred)
    confusion_matrices.append(cm)

avg_cm = np.mean(confusion_matrices, axis=0)
plt.figure(figsize=(6, 4))
sns.heatmap(avg_cm, annot=True, fmt='.2f', cmap='Blues', xticklabels=['chill', 'fear'], yticklabels=['chill', 'fear'])

plt.xlabel('Predito')
plt.ylabel('Verdadeiro')
plt.title('Matriz de Confusão Média')
plt.savefig("confusion_matrix_avg.png")
plt.close()

plt.figure(figsize=(12, 5))

for i, history in enumerate(histories):
    plt.plot(history.history['accuracy'], label=f'Fold {i+1} Treino')
    plt.plot(history.history['val_accuracy'], linestyle='--', label=f'Fold {i+1} Validação')

plt.xlabel('Épocas')
plt.ylabel('Acurácia')
plt.legend()
plt.title('Acurácia do Modelo Otimizado com Cross-Validation')
plt.savefig("accuracy_plots.png")
plt.close()

model.save("eeg_model_optimized_crossval.h5")