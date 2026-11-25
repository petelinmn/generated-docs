---
title: "Подготовка окружения: упражнения"
lang: ru
track: linux
kind: module
doc: exercises
slug: 01-setup
part: 1
tags: [linux, wsl, distribution, terminal, shell, setup]
versions: "Ubuntu 26.04 LTS, Bash 5.3, WSL 2"
status: draft
updated: 2026-11-25
---

# Подготовка окружения: упражнения

🌐 **Русский** · [English](exercises.en.md)

> Сначала решите сами. Опасные шаги выполняйте только в одноразовой среде.

## 1. 🟢 Паспорт системы

Одним блоком команд выведите название дистрибутива, версию ядра, архитектуру и свой числовой идентификатор пользователя.

<details><summary>Подсказка</summary>

`/etc/os-release` можно подключить как shell-файл.

</details>

<details><summary>Решение</summary>

```bash
. /etc/os-release
echo "$PRETTY_NAME"
uname -r
uname -m
id -u
```

```text
Ubuntu 26.04.1 LTS
6.6.87.2-microsoft-standard-WSL2
x86_64
1000
```

`/etc/os-release` состоит из присваиваний `KEY=value`, поэтому команда `.` делает их переменными.

</details>

## 2. 🟡 Найти и исправить CRLF

Создайте два скрипта, один с CRLF. Найдите «плохие» файлы одной командой и исправьте только их.

<details><summary>Подсказка</summary>

`grep -l` печатает имена файлов; символ CR в bash записывается как `$'\r'`.

</details>

<details><summary>Решение</summary>

```bash
printf 'echo a\n' > good.sh
printf 'echo b\r\n' > bad.sh
grep -l $'\r' *.sh
for f in $(grep -l $'\r' *.sh); do sed -i 's/\r$//' "$f"; done
grep -l $'\r' *.sh; echo "left: $?"
```

```text
bad.sh
left: 1
```

Второй `grep` ничего не находит и возвращает код 1. Цикл по выводу команды годится только для имён без пробелов.

</details>

## 3. 🟡 Эксперимент без последствий

Запустите одноразовый контейнер, удалите в нём `/etc/os-release`, выйдите и убедитесь, что новый контейнер цел.

<details><summary>Подсказка</summary>

Флаг `--rm`.

</details>

<details><summary>Решение</summary>

```bash
docker run --rm ubuntu:26.04 sh -c 'rm /etc/os-release; ls /etc/os-release'
docker run --rm ubuntu:26.04 head -n 1 /etc/os-release
```

Первая команда сообщает, что файла нет; вторая запускает контейнер из того же образа, где он на месте. Изменения жили только в слое первого контейнера.

</details>

## 4. 🔴 Копия дистрибутива

Сделайте копию своего дистрибутива под именем `Lab`, создайте в ней файл, убедитесь, что в оригинале его нет, и удалите копию. Перед последней командой дважды проверьте имя.

<details><summary>Подсказка</summary>

`--export`, `--import`, `--unregister`.

</details>

<details><summary>Решение</summary>

```powershell
wsl --export Ubuntu-26.04 $env:TEMP\ubuntu.tar
wsl --import Lab $env:LOCALAPPDATA\wsl\Lab $env:TEMP\ubuntu.tar
wsl -d Lab -- touch /tmp/only-in-lab
wsl -d Ubuntu-26.04 -- ls /tmp/only-in-lab
wsl --unregister Lab      # DESTRUCTIVE: only the clone
```

Копия — независимый дистрибутив. `--unregister` необратим, поэтому имя проверяют по `wsl -l -v`.

</details>
