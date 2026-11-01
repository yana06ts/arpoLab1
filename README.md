# ARPO — Unity 2D Platformer

Учебный проект Unity 6000.3.2f1 на основе 2D Platformer Microgame.

| Работа | Содержание | Отчёт |
| --- | --- | --- |
| ЛР №1 | Автоматическая WebGL-сборка из командной строки | [LR1.md](LR1.md) |
| ЛР №2 | GitHub Actions и резервный репозиторий | [LR2.md](LR2.md) |
| ЛР №3 | Gzip, Nginx, Docker и GHCR | [LR3.md](LR3.md) |

## Локальный запуск

Нужны Docker Desktop с запущенным Linux Engine и готовая сборка в `Builds/WebGL`.

```powershell
# Для установленного на этом компьютере Docker, если команда docker не найдена:
$env:Path += ";$env:LOCALAPPDATA\Programs\DockerDesktop\resources\bin"
docker build -t platformer-compressed:v1 .
docker run -d -p 127.0.0.1:8081:80 --name unity_compressed_container platformer-compressed:v1
```

Открыть http://localhost:8081. Контейнер доступен только на этом компьютере.

После проверки:

```powershell
docker stop unity_compressed_container
docker rm unity_compressed_container
```

## Что хранится в облаке

GitHub Actions запускает три задачи: проверку структуры проекта, копирование текущей ветки в `yana06ts/arpoLab2` и публикацию образа `ghcr.io/yana06ts/platformer-game` под тегами `latest` и SHA коммита.

**По условиям ЛР №3 облачный образ содержит временные заглушки.** Это проверка доставки Docker-образа; полноценная сборка Unity в облаке подключается в следующей работе. Локальный образ собирается из настоящей игры.

Скриншоты этапов: [docs/LR3/screenshots](docs/LR3/screenshots).
