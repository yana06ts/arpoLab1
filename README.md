# ARPO — Unity 2D Platformer

Учебный проект Unity 6000.3.2f1 на основе 2D Platformer Microgame.

| Работа | Тема | Отчёт |
| --- | --- | --- |
| ЛР №3 | Docker и раздача сжатых файлов через Nginx | LR3.md |
| ЛР №4 | Unity Test Runner в CI/CD, вариант 16 | LR4.md |
| ЛР №5 | Облачная сборка WebGL, GitHub Pages, Docker с игрой | LR5.md |
| ЛР №6 | Мониторинг пайплайна и Telegram-оповещения | LR6.md |

## Пайплайн

При каждом push GitHub Actions выполняет задачи по порядку:

1. sanity_check проверяет структуру проекта.
2. run_tests запускает EditMode- и PlayMode-тесты.
3. build_game собирает WebGL-игру в облаке.
4. mirror_repo копирует ветку в резервный репозиторий arpoLab2.
5. build_docker_image публикует образ ghcr.io/yana06ts/platformer-game с игрой.
6. deploy_to_pages публикует игру на GitHub Pages.
7. notify присылает результат прогона в Telegram.

![Пайплайн](docs/LR6/screenshots/01-actions-success.jpg)

Игра на GitHub Pages: yana06ts.github.io/arpoLab1

![Игра](docs/LR5/screenshots/03-pages-game.jpg)
