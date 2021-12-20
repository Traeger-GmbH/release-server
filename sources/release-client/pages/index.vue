<template>
  <Page title="Start">
    <div class="flex flex-col gap-2">
      <LinkCard
        title="Explore"
        link="/products"
        description="Explore, download and manage product releases."
      />
      <LinkCard
        title="Maintain"
        link="/maintain"
        description="View disk usage, backup and restore the database."
      />
      <LinkCard
        title="Upload"
        link="/upload"
        description="Upload complete packages or create a new package."
      />
    </div>
  </Page>
</template>

<script>
export default {
  name: 'IndexPage',
};
</script>
