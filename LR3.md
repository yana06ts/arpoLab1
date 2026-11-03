## 1. Сжатие Gzip в Unity

В Player Settings для WebGL выбран формат сжатия Gzip, Decompression Fallback выключен. После пересборки командой из ЛР1 в папке Builds/WebGL/Build появились файлы WebGL.framework.js.gz, WebGL.wasm.gz и WebGL.data.gz. Три основных файла уменьшились с 64,7 МБ до 20,3 МБ, примерно в 3,2 раза.

![Настройки Unity: Gzip, Decompression Fallback выключен](docs/LR3/screenshots/01-unity-settings.png)

## 2. Конфигурация Nginx

Файл nginx.conf добавляет к файлам .js.gz, .wasm.gz и .data.gz заголовок Content-Encoding: gzip и задаёт правильный Content-Type. Без этого заголовка браузер не может запустить сжатые файлы игры.

![nginx.conf](docs/LR3/screenshots/02-nginx.png)

## 3. Dockerfile

Образ собирается на основе nginx:alpine. Стандартная конфигурация удаляется, копируются свой nginx.conf и файлы игры из Builds/WebGL в /usr/share/nginx/html, открывается порт 80.

![Dockerfile](docs/LR3/screenshots/03-dockerfile.png)

## 4. Локальная проверка контейнера

Образ собран командой docker build -t platformer-compressed:v1 . и запущен командой docker run -d -p 127.0.0.1:8081:80. Порт 8080 занят другим сервером, поэтому используется 8081. Игра открывается по адресу localhost:8081, персонаж управляется с клавиатуры.

![Игра запущена из контейнера](docs/LR3/screenshots/04-game.png)

![Персонаж двигается](docs/LR3/screenshots/05-game-input.png)

В инструментах разработчика у файла WebGL.wasm.gz видны заголовки Content-Encoding: gzip и Content-Type: application/wasm.

![DevTools: Content-Encoding gzip](docs/LR3/screenshots/06-devtools-gzip.png)

## 5. Сборка образа в GitHub Actions

В пайплайн добавлена задача build_docker_image. Папка Builds исключена из Git, поэтому в облаке создаются файлы-заглушки .gz. Затем образ собирается и публикуется в GHCR под тегами latest и SHA коммита. Для задачи заданы права contents: read и packages: write.

![Задача build_docker_image в main.yml](docs/LR3/screenshots/07-workflow.png)

Все три задачи пайплайна выполнены успешно: sanity_check, mirror_repo и build_docker_image.

![GitHub Actions: три задачи зелёные](docs/LR3/screenshots/08-actions-lr3.jpg)

Образ ghcr.io/yana06ts/platformer-game опубликован в GitHub Packages. Начиная с ЛР5 в него вместо заглушек попадает настоящая игра.
