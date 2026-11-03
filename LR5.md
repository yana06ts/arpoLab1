

## 1. Задача build_game

После успешных тестов задача build_game собирает WebGL в контейнере через game-ci/unity-builder. Сборку выполняет метод BuildManager.BuildWebGL. Готовая папка Builds/WebGL загружается в артефакт WebGL-Build.

![Задача build_game в main.yml](docs/LR5/screenshots/08-build-game-job.png)


## 2. Docker-образ и GitHub Pages

Задачи build_docker_image и deploy_to_pages скачивают артефакт WebGL-Build вместо заглушек из ЛР3. Первая собирает образ с Nginx и публикует его в GHCR. Вторая выкладывает игру в ветку gh-pages. В nginx.conf добавлены правила для файлов .unityweb с заголовком Content-Encoding: gzip.

![Задачи build_docker_image и deploy_to_pages](docs/LR5/screenshots/09-docker-pages-jobs.png)

Все задачи пайплайна выполнены успешно, build_game запускается после run_tests.

![Пайплайн ЛР5](docs/LR5/screenshots/01-actions-pipeline.jpg)

## 3. Результаты

В ветке gh-pages лежат index.html, Build, TemplateData и .nojekyll. В папке Build настоящие файлы: WebGL.wasm.unityweb, WebGL.data.unityweb, WebGL.framework.js.unityweb и WebGL.loader.js.

![Ветка gh-pages](docs/LR5/screenshots/02-gh-pages-branch.jpg)

GitHub Pages включён один раз вручную: Settings, Pages, источник gh-pages, папка root.

![Настройки Pages](docs/LR5/screenshots/06-pages-settings.jpg)

Игра открывается по адресу yana06ts.github.io/arpoLab1, персонаж управляется с клавиатуры. Все файлы игры загружаются с кодом 200, ошибок распаковки в консоли нет.

![Игра на GitHub Pages](docs/LR5/screenshots/03-pages-game.jpg)

Пакет platformer-game опубликован в GHCR. Тег latest и тег с SHA указывают на образ последнего коммита.

![Пакет в GHCR](docs/LR5/screenshots/07-ghcr-package.jpg)

Образ скачан из GHCR и запущен локально на порту 8081. Nginx отдаёт WebGL.wasm.unityweb с кодом 200, заголовками Content-Type: application/wasm и Content-Encoding: gzip.

![Игра из Docker-контейнера](docs/LR5/screenshots/04-docker-game.jpg)
