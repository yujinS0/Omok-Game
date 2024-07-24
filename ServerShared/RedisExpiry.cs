﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared;

public static class RedisExpireTime
{
    public static readonly TimeSpan UserLoginInfo = TimeSpan.FromHours(2);
    public static readonly TimeSpan MatchResult = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan GameData = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan PlayingUserInfo = TimeSpan.FromHours(2);
    public static readonly TimeSpan LockTime = TimeSpan.FromSeconds(30);
}