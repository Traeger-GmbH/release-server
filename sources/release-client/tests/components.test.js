import { flushPromises, mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { clearCredentials, setCredentials } from '../src/auth.js';
import DeleteReleaseDialog from '../src/components/DeleteReleaseDialog.vue';
import ReleaseInformation from '../src/components/ReleaseInformation.vue';
import UploadView from '../src/views/UploadView.vue';
import { json, mockFetch, problem } from './helpers.js';

const stubs = { RouterLink: { template: '<a><slot /></a>' }, Teleport: true };

describe('ReleaseInformation', () => {
  const release = {
    version: '1.41',
    platforms: ['windows/any', 'Windows/X86'],
    releaseDate: '2019-02-21T00:00:00',
    isPreviewRelease: false,
    isSecurityPatch: true,
    changes: { de: [{ platforms: null, added: ['Version HW 3 Freigabe'], fixed: [], breaking: null, deprecated: null }], en: [] }
  };

  it('shows German changes if there are no English ones', () => {
    const wrapper = mount(ReleaseInformation, { props: { release, productIdentifier: 'p' }, global: { stubs } });

    expect(wrapper.text()).toContain('Version HW 3 Freigabe');
    expect(wrapper.text()).toContain('security patch');
    expect(wrapper.text()).not.toContain('Fixed:');
  });

  it('offers a public download link per platform', () => {
    const wrapper = mount(ReleaseInformation, { props: { release, productIdentifier: 'p' }, global: { stubs } });

    const links = wrapper.findAll('a[download]').map(link => link.attributes('href'));
    expect(links).toEqual(['/artifacts/p/windows/any/1.41', '/artifacts/p/Windows/X86/1.41']);
  });

  it('switches between languages', async () => {
    const multilingual = { ...release, changes: { de: [{ added: ['Neu'] }], en: [{ added: ['New'] }] } };
    const wrapper = mount(ReleaseInformation, { props: { release: multilingual, productIdentifier: 'p' }, global: { stubs } });

    expect(wrapper.text()).toContain('New');
    await wrapper.findAll('[role="tab"]').find(tab => tab.text() === 'de').trigger('click');
    expect(wrapper.text()).toContain('Neu');
  });

  it('handles releases without changes', () => {
    const wrapper = mount(ReleaseInformation, { props: { release: { ...release, changes: {} }, productIdentifier: 'p' }, global: { stubs } });

    expect(wrapper.text()).toContain('No changes listed.');
  });
});

describe('DeleteReleaseDialog', () => {
  beforeEach(() => setCredentials({ username: 'u', password: 'p' }));
  afterEach(() => clearCredentials());

  const props = { productIdentifier: 'p', release: { version: '1.0', platforms: ['windows/x64'] }, showing: true };

  it('requires the release name before deleting', async () => {
    const fetchMock = mockFetch(() => new Response(null, { status: 200 }));
    const wrapper = mount(DeleteReleaseDialog, { props, global: { stubs } });
    const deleteButton = () => wrapper.findAll('button').find(button => button.text() === 'Delete');

    expect(deleteButton().attributes('disabled')).toBeDefined();
    await wrapper.find('input').setValue('p-v1.0');
    await deleteButton().trigger('click');
    await flushPromises();

    expect(fetchMock).toHaveBeenCalledOnce();
    expect(wrapper.text()).toContain('Successfully deleted');
  });

  it('reports the deletion only after it succeeded', async () => {
    mockFetch(() => problem(500, 'Internal error'));
    const wrapper = mount(DeleteReleaseDialog, { props, global: { stubs } });
    await wrapper.find('input').setValue('p-v1.0');
    await wrapper.findAll('button').find(button => button.text() === 'Delete').trigger('click');
    await flushPromises();
    expect(wrapper.text()).toContain('Internal error');
    await wrapper.findAll('button').find(button => button.text() === 'Finish').trigger('click');

    expect(wrapper.emitted('deleted')).toBeUndefined();
    expect(wrapper.emitted('close')).toHaveLength(1);
  });
});

describe('UploadView', () => {
  beforeEach(() => setCredentials({ username: 'u', password: 'p' }));
  afterEach(() => clearCredentials());

  it('uploads only packages that were not uploaded successfully before', async () => {
    const uploaded = [];
    mockFetch((url, options) => {
      const name = options.body.get('package').name;
      uploaded.push(name);
      return name === 'b.zip' && uploaded.filter(n => n === 'b.zip').length === 1
        ? problem(400, 'Conflict', 'This package already exists.')
        : json({});
    });
    const wrapper = mount(UploadView, { global: { stubs } });
    const dropZone = wrapper.findComponent({ name: 'FileDropZone' });
    dropZone.vm.$emit('add', new File(['a'], 'a.zip'));
    dropZone.vm.$emit('add', new File(['b'], 'b.zip'));
    await flushPromises();
    const uploadButton = () => wrapper.findAll('button').find(button => button.text() === 'upload');

    await uploadButton().trigger('click');
    await flushPromises();
    expect(uploaded).toEqual(['a.zip', 'b.zip']);
    expect(wrapper.text()).toContain('Conflict');

    await wrapper.findAll('input[type="checkbox"]')[2].setValue(true);
    await uploadButton().trigger('click');
    await flushPromises();
    expect(uploaded).toEqual(['a.zip', 'b.zip', 'b.zip']);
  });

  it('sets the overwrite flag for all pending packages', async () => {
    const forced = [];
    mockFetch((url) => { forced.push(url.endsWith('force=true')); return json({}); });
    const wrapper = mount(UploadView, { global: { stubs } });
    const dropZone = wrapper.findComponent({ name: 'FileDropZone' });
    dropZone.vm.$emit('add', new File(['a'], 'a.zip'));
    dropZone.vm.$emit('add', new File(['b'], 'b.zip'));
    await flushPromises();

    await wrapper.findAll('input[type="checkbox"]')[0].setValue(true);
    await wrapper.findAll('button').find(button => button.text() === 'upload').trigger('click');
    await flushPromises();

    expect(forced).toEqual([true, true]);
  });
});

describe('FilterInput', () => {
  it('can shrink to the width of narrow columns', async () => {
    const { default: FilterInput } = await import('../src/components/FilterInput.vue');
    const wrapper = mount(FilterInput, { props: { modelValue: '' } });

    // Without min-width 0 the intrinsic size of the input overflows its flex container (seen in Explore)
    expect(wrapper.classes()).toContain('min-w-0');
    expect(wrapper.find('input').classes()).toEqual(expect.arrayContaining(['min-w-0', 'w-full']));
  });

  it('keeps a single root element, so the layout classes of the parent apply', async () => {
    const { default: FilterInput } = await import('../src/components/FilterInput.vue');
    const wrapper = mount(FilterInput, { props: { modelValue: '' }, attrs: { class: '-mx-4' } });

    expect(wrapper.classes()).toContain('-mx-4');
  });

  it('clears the filter', async () => {
    const { default: FilterInput } = await import('../src/components/FilterInput.vue');
    const wrapper = mount(FilterInput, { props: { modelValue: 'tic' } });

    await wrapper.find('button').trigger('click');

    expect(wrapper.emitted('update:modelValue')).toEqual([['']]);
  });
});
