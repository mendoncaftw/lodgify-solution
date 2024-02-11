### Feedback

_Please add below any feedback you want to send to the team_

- I believe the fix for the GRPC client was the missing the api header

- I believe the fix for the https error on the movies api was the missing dev cert from _my_ machine because it's trusted

- Execution tracking is configured for debug log level, so appsettings.Development.json has that set on the api namespace

- With the transient errors the api generates, I would swap the caching order, checking the cache first and only going to the API on cache expiration / failure. Would also need a method for cache invalidation when the underlying data changes

- If I had a bit more time I would've added some integration tests

- Since IDistributedCache extension methods can't be mocked, I mocked ICacheClient instead

- I hope I understood the assignment correctly, couldn't tell what kind of request was required for Reserving seats: just a number of seats like I did or a list of {row, seat} pairs and then check which ones are contiguous and try to reserve the ones that are.
