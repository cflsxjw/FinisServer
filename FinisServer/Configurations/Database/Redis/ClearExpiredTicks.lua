local expiredEntries = redis.call('ZRANGEBYSCORE', KEYS[1], 0, ARGV[1])
for i, entry in ipairs(expiredEntries) do
    local parts = {}
    for s in string.gmatch(entry, '([^:]+)') do
        table.insert(parts, s)
    end
    if #parts >= 2 then
        local articleId = parts[1]
        local authorId = parts[2]

        -- 检查文章和作者是否还在排行榜里
        local currentArticleScore = redis.call('ZSCORE', KEYS[2], articleId)
        local currentAuthorScore = redis.call('ZSCORE', KEYS[3], articleId)
        if currentArticleScore and currentAuthorScore then
            -- 只有文章和作者还在，才进行正常的滑动窗口减分
            redis.call('ZINCRBY', KEYS[2], -1, articleId)
            redis.call('ZINCRBY', KEYS[3], -1, authorId)

            -- 文章
            local newArticleScore = redis.call('ZSCORE', KEYS[2], articleId)
            if tonumber(newArticleScore) <= 0 then
                redis.call('ZREM', KEYS[2], articleId)
            end
            -- 作者
            local newAuthorScore = redis.call('ZSCORE', KEYS[3], authorId)
            if tonumber(newAuthorScore) <= 0 then
                redis.call('ZREM', KEYS[3], authorId)
                local newAuthorScore = redis.call('ZSCORE', KEYS[3], authorId)
            end
        end
    end
end
redis.call('ZREMRANGEBYSCORE', KEYS[1], 0, ARGV[1])
return #expiredEntries
