cmd-party-set-host-desc = Назначает нового лидера команды
cmd-party-set-host-help = Использоваие: party:sethost <id команды> <имя игрока> [принудительно]

cmd-party-set-host-invalid-arguments-count = Неверное число аргументов!\n{ $help }
cmd-party-set-host-invalid-argument-1 = Не удалось спарсить { $arg } в качестве беззнакового целого числа (uint)
cmd-party-set-host-invalid-party-id = Не удалось найти команду с id { $id }
cmd-party-set-host-invalid-username = Не удалось найти игрока с именем { $username }
cmd-party-set-host-invalid-argument-3 = Не удалось спарсить { $arg } в качестве boolean (True/False)
cmd-party-set-host-user-is-another-party-member = Игрок { $username } уже является участником другого пати. Используйте параметр 'force: True' для принудительного добавления игрока в команду
cmd-party-set-host-members-limit-reached = Целевая команда достигла лимита участников. Используйте параметр 'force: True' для игнорирования лимита участников

cmd-party-set-host-success = Игрок { $username } успешно стал лидером команды { $partyId }!
cmd-party-set-host-fail = Не удалось назначить игрока { $username } лидером команды { $partyId }.

cmd-party-set-host-hint-1 = <id команды>
cmd-party-set-host-hint-2 = <имя игрока>
cmd-party-set-host-hint-3 = [принудительно]
