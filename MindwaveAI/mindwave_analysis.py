import pandas as pd
import matplotlib.pyplot as plt

file_path = "datasets\mindwave_session_video.csv"
data = pd.read_csv(file_path)

data['DeltaBetaRatio'] = data['Delta'] / data['LowBeta']
data['ThetaBetaRatio'] = data['Theta'] / data['LowBeta']

plt.figure(figsize=(14, 12))

plt.subplot(3, 1, 1)
plt.plot(data['DeltaBetaRatio'], label='Delta/Beta', color='b')
plt.plot(data['ThetaBetaRatio'], label='Theta/Beta', color='g')
plt.title('Razão Delta/Beta e Theta/Beta')
plt.xlabel('Amostras')
plt.ylabel('Razão')
plt.legend()

plt.subplot(3, 1, 2)
plt.plot(data['LowAlpha'], label='Low Alpha', color='c')
plt.plot(data['HighAlpha'], label='High Alpha', color='m')
plt.title('Atividade Alfa')
plt.xlabel('Amostras')
plt.ylabel('Potência')
plt.legend()

plt.subplot(3, 1, 3)
plt.plot(data['LowGamma'], label='Low Gamma', color='y')
plt.plot(data['HighGamma'], label='High Gamma', color='r')
plt.title('Atividade Gamma')
plt.xlabel('Amostras')
plt.ylabel('Potência')
plt.legend()

plt.tight_layout()

output_path = "plots\mindwave_session_video_waves.png"
plt.savefig(output_path)
print(f"Gráfico salvo em: {output_path}")

plt.show()

plt.figure(figsize=(14, 6))

plt.plot(data['Meditation'], label='Meditação', color='b')
plt.plot(data['Attention'], label='Atenção', color='g')
plt.title('Meditação e Atenção')
plt.xlabel('Amostras')
plt.ylabel('Nível')
plt.legend()

plt.tight_layout()
output_path_attention = "plots\mindwave_session_video.png"
plt.savefig(output_path_attention)
print(f"Gráfico de Meditação e Atenção salvo em: {output_path_attention}")

plt.show()
