import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.neighbors import KNeighborsClassifier
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt
import seaborn as sns
import joblib

file1 = "datasets\mindwave_session1_flagged.csv"
file2 = "datasets\mindwave_session_chill_flagged.csv"
file3 = "datasets\mindwave_session_flagged2.csv"

df1 = pd.read_csv(file1)
df2 = pd.read_csv(file2)
df3 = pd.read_csv(file3)

data = pd.concat([df1, df2, df3], ignore_index=True)

features = ['Delta', 'Theta', 'LowAlpha', 'HighAlpha', 'LowBeta', 'HighBeta', 'LowGamma', 'HighGamma', 'Attention', 'Meditation', 'EEGValue', 'BlinkStrength']
X = data[features]
y = data['State'].map({'chill': 0, 'fear': 1})

scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

X_train, X_test, y_train, y_test = train_test_split(X_scaled, y, test_size=0.2, random_state=42)

knn = KNeighborsClassifier(n_neighbors=3)
knn.fit(X_train, y_train)

y_pred = knn.predict(X_test)

print(classification_report(y_test, y_pred, target_names=['chill', 'fear']))

cm = confusion_matrix(y_test, y_pred)
plt.figure(figsize=(6, 4))
sns.heatmap(cm, annot=True, fmt='d', cmap='Blues', xticklabels=['chill', 'fear'], yticklabels=['chill', 'fear'])
plt.xlabel('Predito')
plt.ylabel('Verdadeiro')
plt.title('Matriz de Confusão - KNN')
plt.savefig("knn_confusion_matrix.png")
plt.close()

accuracy = knn.score(X_test, y_test)
plt.figure(figsize=(6, 4))
plt.bar(['Acurácia'], [accuracy], color='blue')
plt.ylim(0, 1)
plt.title('Acurácia do Modelo KNN')
plt.ylabel('Acurácia')
plt.savefig("knn_accuracy.png")
plt.close()

joblib.dump(knn, "knn_model.joblib")
print("Modelo salvo como 'knn_model.joblib'")