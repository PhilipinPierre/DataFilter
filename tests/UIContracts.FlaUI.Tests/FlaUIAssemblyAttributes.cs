using Xunit;

// Desktop UI automation must run sequentially (one demo app / input desktop at a time).
[assembly: CollectionBehavior(DisableTestParallelization = true)]
