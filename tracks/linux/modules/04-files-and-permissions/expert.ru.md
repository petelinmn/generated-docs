---
title: "Файлы и права: экспертная теория"
lang: ru
track: linux
kind: module
doc: expert
slug: 04-files-and-permissions
part: 1
tags: [linux, file-permissions, chmod, umask, acl, setuid]
versions: "Ubuntu 26.04 LTS, Bash 5.3, uutils coreutils 0.10"
status: draft
updated: 2026-11-27
---

# Файлы и права: экспертная теория

🌐 **Русский** · [English](expert.en.md)

**Уровни:** [обзор](overview.ru.md) · [1. Базовая](basics.ru.md) · [2. Продвинутая](advanced.ru.md) · **3. Экспертная** · [4. Квиз](quiz.ru.md)

## Под капотом

Режим, UID и GID хранятся в inode (см. [файловую систему](../02-filesystem/expert.ru.md)). При каждом обращении ядро проверяет право поиска (`x`) на каждом каталоге пути, а затем запрошенное право на самой цели. Root обходит большинство проверок за счёт capabilities.

## Справочник

| Восьмеричное | Бит | Файл | Каталог |
|---|---|---|---|
| `4000` | setuid | запуск от имени владельца | в Linux не действует |
| `2000` | setgid | запуск с группой файла | новые записи наследуют группу |
| `1000` | sticky | не действует | ограниченное удаление |
| `400/040/004` | r | чтение | перечисление |
| `200/020/002` | w | запись | изменение записей |
| `100/010/001` | x | выполнение | поиск |

В `ls` буквы `s` и `t` означают специальный бит вместе с `x`; заглавные `S` и `T` — без `x`.

| Команда | Назначение |
|---|---|
| `chmod [-R] mode path` | изменить режим |
| `chown [-R] [-h] user[:group] path` | сменить владельца и группу; `-h` — у самой ссылки |
| `chgrp group path` | сменить группу |
| `umask [-S] [mask]` | показать или задать маску shell |
| `stat -c '%A %a %U:%G' path` | посмотреть |
| `namei -l path` | права всего пути |
| `getfacl`, `setfacl -m/-x/-b/-d` | ACL |
| `lsattr`, `chattr +i/+a` | атрибуты файловой системы: неизменяемый, только дозапись |
| `getcap`, `setcap` | capabilities файла |
| `find -perm 644`, `-perm -002`, `-perm /6000` | точно; все эти биты; любой из этих битов |

## Подвохи

### chown снимает setuid и setgid

```bash
touch su1 && chmod 6755 su1
chown ann su1
stat -c '%A %a' su1
```

```text
-rwxr-xr-x 755
```

### Числовой chmod сохраняет setgid у каталога

```bash
mkdir keep && chmod 2775 keep
chmod 755 keep;   stat -c %a keep
chmod 00755 keep; stat -c %a keep
```

```text
2755
755
```

### chmod без указания «кому» учитывает umask

```bash
touch w && chmod 000 w
( umask 022; chmod +w w )
stat -c %A w
```

```text
--w-------
```

### chmod идёт по символической ссылке

```bash
echo t > target && chmod 600 target && ln -s target link
chmod 644 link
stat -c '%a %n' target
stat -c '%A %n' link
```

```text
644 target
lrwxrwxrwx link
```

Сама ссылка всегда показывает `rwxrwxrwx`.

### Root и выполнение

```bash
echo 'echo hi' > noexec && chmod 000 noexec
cat noexec          # root reads it anyway
./noexec            # but cannot execute a file with no x bit at all
bash noexec         # bash only reads it
```

```text
echo hi
bash: ./noexec: Permission denied
hi
```

### Закрытый каталог выше по пути

```bash
mkdir -p path/one && echo z > path/one/z && chmod 700 path
su ann -c 'namei -l /lab/path/one/z'
```

```text
f: /lab/path/one/z
drwxr-xr-x root root /
drwxr-xr-x root root lab
drwx------ root root path
                      one - Permission denied
```

### Неизменяемые файлы

```bash
touch im && chattr +i im      # needs root (CAP_LINUX_IMMUTABLE)
rm -f im
lsattr im
```

```text
rm: cannot remove 'im': Operation not permitted
----i---------e------- im
```

Контейнер Docker по умолчанию не даёт этой capability, поэтому `chattr +i` там не работает.

### Копии

```bash
touch p && chmod 751 p
( umask 022; cp p p2; cp -p p p3 )
install -m 600 -o ann p p4
stat -c '%a %U %n' p2 p3 p4
```

```text
751 root p2
751 root p3
600 ann p4
```

`cp` переносит биты режима с учётом umask; `cp -p` сохраняет ещё владельца и время, когда это разрешено; `install` задаёт режим и владельца явно.

## Компромиссы и альтернативы

| Механизм | Плюс | Минус |
|---|---|---|
| Биты режима | просто, везде | один владелец и одна группа |
| ACL | тонкая настройка | легко не заметить, маска |
| Правила `sudo` | управляемо и журналируется | настройка |
| Capabilities | меньше полного root | труднее проверять |
| MAC (AppArmor, SELinux) | ограничивает даже процессы root | политики |

## Вопросы с собеседований

**1. Что означает `755`?**

<details><summary>Ответ</summary>

Владелец `rwx`, группа и остальные `r-x`.

</details>

**2. Что значат `r`, `w`, `x` у каталога?**

<details><summary>Ответ</summary>

Перечислять имена; создавать и удалять записи; проходить и обращаться по имени.

</details>

**3. Почему пользователь может удалить чужой файл только для чтения?**

<details><summary>Ответ</summary>

Удаление меняет каталог; достаточно записи и выполнения на нём, если нет sticky.

</details>

**4. Как вычисляется режим нового файла?**

<details><summary>Ответ</summary>

Запрошенный режим с очищенными битами `umask` (ACL по умолчанию заменяет umask).

</details>

**5. Что делает setuid?**

<details><summary>Ответ</summary>

Запускает программу с эффективным UID владельца.

</details>

**6. Зачем sticky bit?**

<details><summary>Ответ</summary>

Чтобы пользователи общего каталога не удаляли файлы друг друга, как в `/tmp`.

</details>

**7. Что такое маска ACL?**

<details><summary>Ответ</summary>

Максимум эффективных прав именованных пользователей, групп и группы-владельца.

</details>

**8. Подвох: у владельца прав нет, а у группы есть. Прочитает ли владелец?**

<details><summary>Ответ</summary>

Нет: применяется только класс владельца. Но он может изменить режим.

</details>

**9. Подвох: запустит ли root файл без единого `x`?**

<details><summary>Ответ</summary>

Нет.

</details>

**10. Как понять, почему отказано в доступе?**

<details><summary>Ответ</summary>

Проверить каждый каталог пути (`namei -l`), владельца, группу, ACL, текущие группы пользователя (`id`), атрибуты файла и параметры монтирования.

</details>

**11. Что такое capabilities?**

<details><summary>Ответ</summary>

Отдельные части привилегий root, которые можно выдавать по одной.

</details>

## Связанные материалы

- [Продвинутая теория](advanced.ru.md)
- [Квиз](quiz.ru.md) и [упражнения](exercises.ru.md)

## Источники

- [chmod(1)](https://man7.org/linux/man-pages/man1/chmod.1.html)
- [chown(1)](https://man7.org/linux/man-pages/man1/chown.1.html)
- [umask(2)](https://man7.org/linux/man-pages/man2/umask.2.html)
- [path_resolution(7)](https://man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://man7.org/linux/man-pages/man7/inode.7.html)
- [execve(2)](https://man7.org/linux/man-pages/man2/execve.2.html)
- [chown(2)](https://man7.org/linux/man-pages/man2/chown.2.html)
- [unlink(2)](https://man7.org/linux/man-pages/man2/unlink.2.html)
- [setfacl(1)](https://man7.org/linux/man-pages/man1/setfacl.1.html)
- [acl(5)](https://man7.org/linux/man-pages/man5/acl.5.html)
- [chattr(1)](https://man7.org/linux/man-pages/man1/chattr.1.html)
- [capabilities(7)](https://man7.org/linux/man-pages/man7/capabilities.7.html)
