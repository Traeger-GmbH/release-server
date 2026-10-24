<template>
  <PageLayout title="Start">
    <div class="flex flex-col gap-2">
      <LinkCard title="Explore" link="/products" description="Explore, download and manage product releases." />
      <LinkCard title="Maintain" link="/maintain" description="View disk usage, backup and restore the data." />
      <LinkCard title="Upload" link="/upload" description="Upload release packages." />
    </div>
  </PageLayout>
</template>

<script setup>
import LinkCard from '../components/LinkCard.vue';
import PageLayout from '../components/PageLayout.vue';
</script>
