cmd-party-add-member-desc = Добавляет игрока в команду
cmd-party-add-member-help = Использование: party:adduser <id команды> <имя игрока> [принудительно]

cmd-party-add-member-invalid-arguments-count = Неверное число аргументов!\n{ $help }
cmd-party-add-member-invalid-argument-1 = Не удалось спарсить { $arg } в качестве беззнакового целого числа (uint)
cmd-party-add-member-invalid-party-id = Не удалось найти команду с id { $id }
cmd-party-add-member-invalid-username = Не удалось найти игрока с именем { $username }
cmd-party-add-member-invalid-argument-3 = Не удалось спарсить { $arg } в качестве boolean (True/False)
cmd-party-add-member-user-is-another-party-member = Пользователь { $username } уже является участником другой команды. Используйте параметр 'force: True' для принудительного добавления игрока в пати
cmd-party-add-member-members-limit-reached = Целевая команда достигла лимита участников. Используйте параметр 'force: True' для игнорирования лимита участников

cmd-party-add-member-success = Игрок { $username } успешно добавлен в команду { $partyId }!
cmd-party-add-member-fail = Не удалось добавить игрока { $username } в команду { $partyId }.

cmd-party-add-member-hint-1 = <id команды>
cmd-party-add-member-hint-2 = <имя игрока>
cmd-party-add-member-hint-3 = [принудительно]
