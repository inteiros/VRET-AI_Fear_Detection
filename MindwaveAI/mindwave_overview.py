import pandas as pd
import matplotlib.pyplot as plt

file_path = "datasets\mindwave_session1.csv"

data = pd.read_csv(file_path)

plt.figure(figsize=(14, 10))

plt.subplot(3, 1, 1)
plt.plot(data['Delta'], label='Delta')
plt.plot(data['LowBeta'], label='Low Beta')
plt.title('Delta vs Low Beta')
plt.xlabel('Amostras')
plt.ylabel('Potência')
plt.legend()

plt.subplot(3, 1, 2)
plt.plot(data['Theta'], label='Theta')
plt.title('Theta')
plt.xlabel('Amostras')
plt.ylabel('Potência')
plt.legend()

plt.subplot(3, 1, 3)
plt.plot(data['LowAlpha'], label='Low Alpha')
plt.plot(data['HighAlpha'], label='High Alpha')
plt.plot(data['LowGamma'], label='Low Gamma')
plt.plot(data['HighGamma'], label='High Gamma')
plt.title('Alpha e Gamma')
plt.xlabel('Amostras')
plt.ylabel('Potência')
plt.legend()

plt.tight_layout()

output_path = "datasets\mindwave_overview.png"
plt.savefig(output_path)
print(f"Gráfico salvo em: {output_path}")

plt.show()
