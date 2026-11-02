# Лабораторная работа №5

**Тема:** автоматическая компиляция WebGL-игры в облаке и деплой на GitHub Pages.

**Проект:** Unity 6000.3.2f1, 2D Platformer Microgame.
**Репозиторий:** https://github.com/yana06ts/arpoLab1
**Игра на GitHub Pages:** https://yana06ts.github.io/arpoLab1/
**Docker-образ:** `ghcr.io/yana06ts/platformer-game:latest`

## 1. Настройки Unity для GitHub Pages

В `ProjectSettings/ProjectSettings.asset`:

```text
webGLCompressionFormat: 1        # Gzip (0 = Brotli, 1 = Gzip, 2 = Disabled)
webGLDecompressionFallback: 1    # резервная декомпрессия включена
```

В методичке для Gzip указано значение `0`. В Unity `0` означает Brotli, а для Gzip используется `1`. Проверка:

```powershell
Select-String -Path ProjectSettings\ProjectSettings.asset -Pattern "webGLCompressionFormat|webGLDecompressionFallback"
```

GitHub Pages не отправляет заголовок `Content-Encoding: gzip`. Поэтому Unity добавляет в сборку JS-распаковщик. Со включённым Decompression Fallback файлы называются `*.unityweb`, например `Build/WebGL.wasm.unityweb`. Внутри у них тот же Gzip.

## 2. Пайплайн

Файл: [.github/workflows/main.yml](.github/workflows/main.yml).

```text
sanity_check → run_tests ─┬→ mirror_repo
                          └→ build_game ─┬→ build_docker_image → GHCR
                                         └→ deploy_to_pages    → ветка gh-pages
```

| Задача | Что делает |
| --- | --- |
| `sanity_check` | Проверяет структуру проекта, `.asmdef` и файлы доставки |
| `run_tests` (ЛР4) | Запускает EditMode- и PlayMode-тесты в контейнере `unityci/editor` |
| `build_game` | Использует `game-ci/unity-builder@v4` с `buildMethod: BuildManager.BuildWebGL` и `unityVersion: 6000.3.2f1`. Результат загружается в артефакт `WebGL-Build` |
| `mirror_repo` (ЛР2) | Копирует ветку в `yana06ts/arpoLab2` после успешных тестов |
| `build_docker_image` | Скачивает `WebGL-Build` вместо заглушек `touch` из ЛР3 и публикует образ с игрой в GHCR (`latest` и SHA коммита) |
| `deploy_to_pages` | Скачивает `WebGL-Build`, добавляет `.nojekyll` и через `JamesIves/github-pages-deploy-action@v4` публикует в ветку `gh-pages` |

`BuildManager.cs` перекодирован из Windows-1251 в UTF-8. В облачном Linux-контейнере русские сообщения раньше выводились бы «кракозябрами». Теперь в логе `build_game` читается строка `[CI/CD] УСПЕХ! WebGL билд успешно создан.` со временем и размером сборки. Метод `BuildWebGL` вызывает `EditorApplication.Exit(0/1)`, поэтому пайплайн получает код результата.

## 3. Nginx и Docker

В [nginx.conf](nginx.conf) добавлены правила для `*.js.unityweb`, `*.wasm.unityweb` и `*.data.unityweb`. Для них сервер отдаёт правильный `Content-Type` и `Content-Encoding: gzip`. В Docker распаковку выполняет сам браузер, это быстрее JS-распаковщика. На GitHub Pages заголовка нет, и работает распаковщик Unity. Правила ЛР3 для `*.gz` оставлены.

Конфигурация проверена командой `nginx -t`. Тестовый сервер вернул для `.wasm.unityweb` `Content-Type: application/wasm` и `Content-Encoding: gzip`, а для несуществующего файла — 404.

Проверка опубликованного образа:

```powershell
docker run -d -p 127.0.0.1:8081:80 --name platformer ghcr.io/yana06ts/platformer-game:latest
curl.exe -I -H "Accept-Encoding: gzip" http://localhost:8081/Build/WebGL.wasm.unityweb
docker stop platformer; docker rm platformer
```

## 4. Включение GitHub Pages (один раз вручную)

1. Репозиторий → **Settings → Pages**.
2. **Source:** Deploy from a branch.
3. **Branch:** `gh-pages`, папка `/ (root)` → Save.
4. Через 1–2 минуты появится ссылка https://yana06ts.github.io/arpoLab1/.

## 5. Проверка результатов

**1. Пайплайн.** Первый прогон ветки `LR5` (#6) упал на `run_tests`, потому что ещё не было секретов Unity. После их добавления прогон #7 полностью зелёный: `build_game` стартовал только после `run_tests`, а `build_docker_image` и `deploy_to_pages` выполнились после `build_game`. Сборка в облаке заняла около 22 минут, весь прогон — 27,5 минуты.

![Прогон #7: все задачи ЛР5 зелёные](docs/LR5/screenshots/01-actions-pipeline.jpg)

В логе `build_game` есть строки метода `BuildManager.BuildWebGL` из ЛР1. Это значит, что сборку выполнил именно он, а не стандартная сборка GameCI. Время сборки 703,68 с, размер 20 380 970 байт. Ниже — фрагмент лога (прогон #8 ветки LR6, тот же пайплайн):

![Лог build_game: [CI/CD] УСПЕХ!](docs/LR5/screenshots/05-build-log-success.png)

**2–3. Артефакт и ветка gh-pages.** В ветку `gh-pages` выгружены `index.html`, `Build`, `TemplateData` и `.nojekyll`. Внутри `Build` лежат настоящие файлы, а не пустые заглушки:

| Файл | Размер |
| --- | ---: |
| WebGL.wasm.unityweb | 12 021 354 байт |
| WebGL.data.unityweb | 8 204 112 байт |
| WebGL.framework.js.unityweb | 88 320 байт |
| WebGL.loader.js | 47 866 байт |

![Ветка gh-pages](docs/LR5/screenshots/02-gh-pages-branch.jpg)

**4. Игра на GitHub Pages.** Pages включён в Settings → Pages (источник `gh-pages`, `/ (root)`):

![Settings → Pages](docs/LR5/screenshots/06-pages-settings.jpg)

Игра по адресу https://yana06ts.github.io/arpoLab1/ загружается, персонаж управляется стрелками:

![Игра на GitHub Pages](docs/LR5/screenshots/03-pages-game.jpg)

**5. Сеть и консоль.** Все файлы игры загружаются с кодом 200, ошибок распаковки в консоли нет:

```text
GET /arpoLab1/                              → 200
GET /arpoLab1/Build/WebGL.loader.js         → 200
GET /arpoLab1/Build/WebGL.framework.js.unityweb → 200
GET /arpoLab1/Build/WebGL.wasm.unityweb     → 200
GET /arpoLab1/Build/WebGL.data.unityweb     → 200
GET /arpoLab1/TemplateData/*                → 200
Console errors: нет
```

**6. Docker-образ из GHCR.** Пакет `platformer-game` есть в Packages. Тег `latest` и тег с SHA указывают на один и тот же образ последнего коммита:

![Пакет platformer-game в GHCR](docs/LR5/screenshots/07-ghcr-package.jpg)

Образ скачан из реестра и запущен локально на порту 8081. Nginx отдаёт файлы игры с `Content-Encoding: gzip` и правильным `Content-Type`. Полный вывод команд — в [docs/LR5/docker-check.txt](docs/LR5/docker-check.txt), фрагмент:

```text
--- curl -I -H "Accept-Encoding: gzip" http://localhost:8081/Build/WebGL.wasm.unityweb
HTTP/1.1 200 OK
Server: nginx/1.31.8
Content-Type: application/wasm
Content-Length: 12021354
Content-Encoding: gzip
```

![Игра из Docker-контейнера на localhost:8081](docs/LR5/screenshots/04-docker-game.jpg)

## Как это работает простыми словами

Раньше игру собирали на своём компьютере, а в облако отправляли пустые заглушки. Теперь после каждого push GitHub запускает Unity в контейнере. Сначала проходят тесты. Если они зелёные, Unity собирает настоящую WebGL-игру методом из ЛР1. Готовые файлы сохраняются как артефакт и передаются двум задачам: одна публикует сайт на GitHub Pages, другая упаковывает игру с Nginx в Docker-образ.

## Контрольные вопросы

**1. Почему Builds/WebGL в .gitignore и как файлы попадают в другие задачи?**
Сборка — это большие сгенерированные файлы, которые можно воссоздать из исходников. Хранить их в Git бессмысленно. `build_game` загружает папку в артефакт `WebGL-Build` (`upload-artifact`), а `build_docker_image` и `deploy_to_pages` скачивают его (`download-artifact`).

**2. Зачем build_game нужен needs: run_tests?**
Чтобы долгая сборка запускалась только для проверенного кода. Если тесты падают, `build_game` пропускается. Вместе с ним пропускаются `build_docker_image` и `deploy_to_pages`, потому что они зависят от `build_game`. `mirror_repo` тоже пропускается, так как зависит от `run_tests`.

**3. Какую роль играет BuildManager.BuildWebGL?**
Метод берёт сцены из Build Settings, собирает WebGL в `Builds/WebGL` и пишет в лог результат, время и размер. Затем он вызывает `EditorApplication.Exit(0)` при успехе и `Exit(1)` при ошибке. По этому коду завершения Unity GameCI и GitHub определяют, успешна ли задача.

**4. Почему Pages нужен Decompression Fallback, а Nginx — нет?**
Nginx можно настроить так, чтобы он добавлял `Content-Encoding: gzip`, и браузер распакует файлы сам. На GitHub Pages заголовки настраивать нельзя. Без fallback браузер получил бы сжатые байты как обычные данные, и загрузка остановилась бы с ошибкой. Fallback встраивает JS-распаковщик.

**5. Чем Pages отличается от Docker-образа, зачем latest и SHA?**
Pages — бесплатный статический хостинг для демонстрации. Docker-образ содержит игру вместе с настроенным веб-сервером и запускается на любом сервере с Docker. `latest` — последняя версия. Тег с SHA привязан к конкретному коммиту и позволяет вернуться к нужной сборке.

**6. Почему после успешного deploy_to_pages сайт может отдавать 404?**
Задача только записывает файлы в ветку `gh-pages`, но не включает хостинг. Нужно один раз выбрать в Settings → Pages источник `gh-pages / (root)`.
