<script setup lang="ts">
import {withBase, useData, useRouter} from 'vitepress'
import localSearchIndex from '@localSearchIndex'
import {shallowRef, ref, onMounted} from "vue"
import MiniSearch, {type SearchResult} from "minisearch"

const {theme, localeIndex} = useData()

interface Result {
  title: string
  titles: string[]
  text?: string
}

const searchIndexData = shallowRef(localSearchIndex)

// hmr
if (import.meta.hot) {
  import.meta.hot.accept('/@localSearchIndex', (m) => {
    if (m) {
      searchIndexData.value = m.default
    }
  })
}

const results = ref([] as (SearchResult & Result)[])

const redirects = {
  "/articles/faq.html": "/intro/faq.html",
  "/articles/quickstart.html": "/intro/quick-start.html",
  "/articles/intro.html": "/intro/installation.html",
  "/articles/profiles.html": "/migration-types/profiles.html",
  "/articles/migration-runners.html": "/runners/index.html",
} as const;

onMounted(async () => {
  // Check redirected pages
  const router = useRouter();
  const currentRoute = router.route;

  for (const [oldPath, newPath] of Object.entries(redirects)) {
    const oldFullPath = withBase(oldPath);
    if (currentRoute.path.startsWith(oldFullPath)) {
      document.location.replace(withBase(newPath))
      return;
    }
  }

  // Load search index if no exact match found
  const indexData = (await searchIndexData.value[localeIndex.value]?.())?.default;

  const indexOptions = {
    fields: ['title', 'titles', 'text'],
    storeFields: ['title', 'titles'],
    searchOptions: {
      fuzzy: 0.1,
      prefix: true,
      boost: {title: 4, text: 2, titles: 1},
      ...theme.value.search.options?.miniSearch?.searchOptions
    },
    ...theme.value.search.options?.miniSearch?.options
  } as const;

  const searchIndex = MiniSearch.loadJSON<Result>(indexData, indexOptions);

  const base = withBase('/');
  const search = document.location.pathname.substring(base.length);

  results.value = searchIndex
      .search(search)
      .slice(0, 4) as (SearchResult & Result)[]
})
</script>

<template>
  <div class="container">
    <div class="NotFound">
      <p class="code">{{ theme.notFound?.code ?? '404' }}</p>
      <h1 class="title">{{ theme.notFound?.title ?? 'PAGE NOT FOUND' }}</h1>
      <div class="divider"/>
      <blockquote class="quote">
        It looks like you lost your way.
        <p v-if="results.length">May I suggest results that may interest you?</p>
      </blockquote>

      <div class="matching vp-doc">
        <ul
            :role="results?.length ? 'listbox' : undefined"
            :aria-labelledby="results?.length ? 'localsearch-label' : undefined"
            class="results"
        >
          <li
              v-for="(p, index) in results"
              :key="p.id"
              role="option"
          >
            <a
                :href="p.id"
                :aria-label="[...p.titles, p.title].join(' > ')"
                :data-index="index"
            >
            <span
                v-for="(t, index) in p.titles"
                :key="index"
            >
              <span class="text">{{t}}</span>
              <span class="vpi-chevron-right local-search-icon"/>
            </span>
              <span class="main">
                <span class="text">{{p.title}}</span>
            </span>
            </a>
          </li>
        </ul>
      </div>

      <div class="action">
        <a
            class="link"
            :href="withBase('/')"
            :aria-label="theme.notFound?.linkLabel ?? 'go to home'"
        >
          {{ theme.notFound?.linkText ?? 'Take me home' }}
        </a>
      </div>
    </div>
  </div>
</template>

<style scoped>
.container {
  margin: auto;
  width: 100%;
  max-width: 1280px;
  padding: 0 24px;
}

@media (min-width: 640px) {
  .container {
    padding: 0 64px;
  }
}

@media (min-width: 960px) {
  .container {
    width: 100%;
    padding: 0 128px;
  }
}

.NotFound {
  padding: 32px;
  text-align: center;
}

@media (min-width: 768px) {
  .NotFound {
    padding: 16px;
  }
}

.code {
  line-height: 64px;
  font-size: 64px;
  font-weight: 600;
}

.title {
  padding-top: 12px;
  letter-spacing: 2px;
  line-height: 20px;
  font-size: 20px;
  font-weight: 700;
}

.divider {
  margin: 24px auto 18px;
  width: 64px;
  height: 1px;
  background-color: var(--vp-c-divider);
}

.local-search-icon {
  display: inline-block;
}

.quote {
  margin: 0 auto;
  font-size: 16px;
  font-weight: 500;
  color: var(--vp-c-text-2);
}

.action {
  padding-top: 20px;
}

.link {
  display: inline-block;
  border: 1px solid var(--vp-c-brand-1);
  border-radius: 16px;
  padding: 3px 16px;
  font-weight: 500;
  color: var(--vp-c-brand-1);
  transition: border-color 0.25s,
  color 0.25s;
}

.link:hover {
  border-color: var(--vp-c-brand-2);
  color: var(--vp-c-brand-2);
}

.matching {
  text-align: left;
  font-size: 16px;
}

.results {
  overflow-x: hidden;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.result > div {
  margin: 12px;
  width: 100%;
  overflow: hidden;
}
</style>
