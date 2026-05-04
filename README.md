# Yfight

Jeu de combat 2 joueurs en C# / .NET 10 avec Raylib.

## Lancer le jeu

```
dotnet run
```

## Contrôles

| Action | Touche |
|---|---|
| Déplacer | Flèches gauche / droite |
| Sauter | Flèche haut |
| Grimper | Flèche haut (sur échelle) |
| Viser | Souris |
| Tirer / Utiliser | Clic gauche |
| Ramasser coffre | E |
| Changer de slot | 1 / 2 / 3 |
| Plein écran | F11 |

**1 joueur (bot) :** P1 joue contre l'IA — flèches + souris.

**2 joueurs :** P1 flèches + souris, P2 WASD. Seul P1 a un pistolet.

**En ligne :** chaque joueur joue sur sa propre machine avec les mêmes contrôles.

## Jouer en ligne (même WiFi)

> Les deux machines doivent être sur le **même réseau WiFi**.

1. **Hôte** — cliquer sur `En ligne → Héberger`. L'IP locale s'affiche à l'écran.
2. **Client** — cliquer sur `En ligne → Rejoindre`, entrer l'IP affichée, puis `Connecter`.
3. La partie démarre automatiquement une fois les deux connectés.

Si ça reste bloqué sur la connexion :
- Vérifier que l'hôte a lancé le jeu **en premier**.
- Autoriser le jeu sur le **port UDP 7777** dans le pare-feu Windows.
- Utiliser exactement l'IP affichée sur l'écran de l'hôte.
